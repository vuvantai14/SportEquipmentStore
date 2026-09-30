using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Enums;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Core.Models.Orders;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class OrderService : IOrderService
{
    private readonly SportEquipmentStoreDbContext _context;

    public OrderService(SportEquipmentStoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .ThenInclude(customer => customer.User)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        return OrdersWithDetails()
            .SingleOrDefaultAsync(order => order.OrderId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));

        return await OrdersWithDetails()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> CreateOrderFromCartAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCreateRequest(request);

        var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        try
        {
            var existingOrder = await _context.Orders
                .AsNoTracking()
                .Include(order => order.OrderDetails)
                .ThenInclude(detail => detail.Product)
                .SingleOrDefaultAsync(
                    order => order.CustomerId == request.CustomerId
                        && order.CheckoutRequestId == request.CheckoutRequestId,
                    cancellationToken);

            if (existingOrder is not null)
            {
                await CommitIfOwnedAsync(transaction, cancellationToken);
                return existingOrder;
            }

            var customer = await _context.Customers
                .SingleOrDefaultAsync(item => item.CustomerId == request.CustomerId, cancellationToken)
                ?? throw new KeyNotFoundException($"Customer {request.CustomerId} was not found.");

            var cart = await _context.Carts
                .Include(item => item.CartItems)
                .ThenInclude(item => item.Product)
                .ThenInclude(product => product.Category)
                .SingleOrDefaultAsync(item => item.CustomerId == request.CustomerId, cancellationToken)
                ?? throw new InvalidOperationException("The customer does not have a cart.");

            if (cart.CartItems.Count == 0)
            {
                throw new InvalidOperationException("The cart is empty.");
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                Customer = customer,
                CheckoutRequestId = request.CheckoutRequestId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingFullName = request.ShippingFullName.Trim(),
                ShippingPhone = request.ShippingPhone.Trim(),
                ShippingAddress = request.ShippingAddress.Trim(),
                Note = ServiceValidation.OptionalText(request.Note, 1000, nameof(request.Note))
            };

            decimal totalAmount = 0;
            foreach (var cartItem in cart.CartItems)
            {
                ValidateCartItemForOrder(cartItem);

                var unitPrice = cartItem.Product.Price;
                var lineTotal = checked(cartItem.Quantity * unitPrice);
                totalAmount = checked(totalAmount + lineTotal);

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = cartItem.ProductId,
                    Product = cartItem.Product,
                    ProductNameAtPurchase = cartItem.Product.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                });

                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await CommitIfOwnedAsync(transaction, cancellationToken);

            return order;
        }
        catch
        {
            await RollbackIfOwnedAsync(transaction, cancellationToken);
            throw;
        }
        finally
        {
            await DisposeIfOwnedAsync(transaction);
        }
    }

    public Task<Order> UpdateStatusAsync(
        int orderId,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(orderId, nameof(orderId));
        return ChangeStatusAsync(orderId, newStatus, null, cancellationToken);
    }

    public Task<Order> CancelByCustomerAsync(
        int orderId,
        int customerId,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(orderId, nameof(orderId));
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));
        return ChangeStatusAsync(orderId, OrderStatus.Cancelled, customerId, cancellationToken);
    }

    private async Task<Order> ChangeStatusAsync(
        int orderId,
        OrderStatus newStatus,
        int? customerId,
        CancellationToken cancellationToken)
    {
        var transaction = await BeginTransactionIfNeededAsync(cancellationToken);
        try
        {
            var order = await _context.Orders
                .Include(item => item.OrderDetails)
                .ThenInclude(detail => detail.Product)
                .SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Order {orderId} was not found.");

            if (customerId.HasValue && order.CustomerId != customerId.Value)
            {
                throw new InvalidOperationException("The order does not belong to this customer.");
            }

            if (order.Status == newStatus)
            {
                await CommitIfOwnedAsync(transaction, cancellationToken);
                return order;
            }

            if (customerId.HasValue)
            {
                if (order.Status != OrderStatus.Pending || newStatus != OrderStatus.Cancelled)
                {
                    throw new InvalidOperationException("A customer can cancel only a Pending order.");
                }
            }
            else if (!IsValidStatusTransition(order.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"Order status cannot change from {order.Status} to {newStatus}.");
            }

            if (newStatus == OrderStatus.Cancelled)
            {
                RestoreStock(order);
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);
            await CommitIfOwnedAsync(transaction, cancellationToken);

            return order;
        }
        catch
        {
            await RollbackIfOwnedAsync(transaction, cancellationToken);
            throw;
        }
        finally
        {
            await DisposeIfOwnedAsync(transaction);
        }
    }

    private IQueryable<Order> OrdersWithDetails()
    {
        return _context.Orders
            .AsNoTracking()
            .Include(order => order.Customer)
            .ThenInclude(customer => customer.User)
            .Include(order => order.OrderDetails)
            .ThenInclude(detail => detail.Product);
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipping) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipping, OrderStatus.Completed) => true,
            _ => false
        };
    }

    private static void RestoreStock(Order order)
    {
        foreach (var detail in order.OrderDetails)
        {
            detail.Product.StockQuantity = checked(detail.Product.StockQuantity + detail.Quantity);
        }
    }

    private static void ValidateCartItemForOrder(CartItem cartItem)
    {
        if (cartItem.Quantity <= 0)
        {
            throw new InvalidOperationException(
                $"Cart item {cartItem.CartItemId} has an invalid quantity.");
        }

        if (!cartItem.Product.IsActive)
        {
            throw new InvalidOperationException($"Product {cartItem.ProductId} is inactive.");
        }

        if (!cartItem.Product.Category.IsActive)
        {
            throw new InvalidOperationException(
                $"Category {cartItem.Product.CategoryId} for product {cartItem.ProductId} is inactive.");
        }

        if (cartItem.Quantity > cartItem.Product.StockQuantity)
        {
            throw new InvalidOperationException(
                $"Product {cartItem.ProductId} does not have enough stock.");
        }
    }

    private static void ValidateCreateRequest(CreateOrderRequest request)
    {
        ServiceValidation.RequirePositiveId(request.CustomerId, nameof(request.CustomerId));

        if (request.CheckoutRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "CheckoutRequestId must be provided for idempotent checkout.",
                nameof(request.CheckoutRequestId));
        }

        ServiceValidation.RequiredText(request.ShippingFullName, 150, nameof(request.ShippingFullName));
        ServiceValidation.RequiredText(request.ShippingPhone, 20, nameof(request.ShippingPhone));
        ServiceValidation.RequiredText(request.ShippingAddress, 500, nameof(request.ShippingAddress));
        ServiceValidation.OptionalText(request.Note, 1000, nameof(request.Note));
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNeededAsync(
        CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            return null;
        }

        return await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
    }

    private static async Task CommitIfOwnedAsync(
        IDbContextTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private static async Task RollbackIfOwnedAsync(
        IDbContextTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    private static async ValueTask DisposeIfOwnedAsync(IDbContextTransaction? transaction)
    {
        if (transaction is not null)
        {
            await transaction.DisposeAsync();
        }
    }
}
