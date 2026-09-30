using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class CartService : ICartService
{
    private readonly SportEquipmentStoreDbContext _context;

    public CartService(SportEquipmentStoreDbContext context)
    {
        _context = context;
    }

    public Task<Cart?> GetCartByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));

        return _context.Carts
            .AsNoTracking()
            .Include(cart => cart.CartItems)
            .ThenInclude(item => item.Product)
            .ThenInclude(product => product.Category)
            .SingleOrDefaultAsync(cart => cart.CustomerId == customerId, cancellationToken);
    }

    public async Task<CartItem> AddItemAsync(
        int customerId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        ValidateIdentifiersAndQuantity(customerId, productId, quantity);
        var transaction = await BeginTransactionIfNeededAsync(cancellationToken);

        try
        {
            if (!await _context.Customers.AnyAsync(
                    customer => customer.CustomerId == customerId,
                    cancellationToken))
            {
                throw new KeyNotFoundException($"Customer {customerId} was not found.");
            }

            var product = await GetPurchasableProductAsync(productId, cancellationToken);
            var cart = await _context.Carts
                .Include(item => item.CartItems)
                .SingleOrDefaultAsync(item => item.CustomerId == customerId, cancellationToken);

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.SingleOrDefault(item => item.ProductId == productId);
            var totalQuantity = quantity;

            if (cartItem is null)
            {
                cartItem = new CartItem
                {
                    Cart = cart,
                    Product = product,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                totalQuantity = CheckedAdd(cartItem.Quantity, quantity);
                cartItem.Quantity = totalQuantity;
                cartItem.Product = product;
            }

            EnsureStock(product, totalQuantity);
            await _context.SaveChangesAsync(cancellationToken);
            await CommitIfOwnedAsync(transaction, cancellationToken);

            return cartItem;
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

    public async Task<CartItem> UpdateQuantityAsync(
        int customerId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        ValidateIdentifiersAndQuantity(customerId, productId, quantity);
        var transaction = await BeginTransactionIfNeededAsync(cancellationToken);

        try
        {
            var cartItem = await _context.CartItems
                .Include(item => item.Product)
                .ThenInclude(product => product.Category)
                .SingleOrDefaultAsync(
                    item => item.Cart.CustomerId == customerId && item.ProductId == productId,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Product {productId} was not found in customer {customerId}'s cart.");

            EnsurePurchasable(cartItem.Product);
            EnsureStock(cartItem.Product, quantity);
            cartItem.Quantity = quantity;

            await _context.SaveChangesAsync(cancellationToken);
            await CommitIfOwnedAsync(transaction, cancellationToken);

            return cartItem;
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

    public async Task<bool> RemoveItemAsync(
        int customerId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));
        ServiceValidation.RequirePositiveId(productId, nameof(productId));

        var cartItem = await _context.CartItems
            .SingleOrDefaultAsync(
                item => item.Cart.CustomerId == customerId && item.ProductId == productId,
                cancellationToken);

        if (cartItem is null)
        {
            return false;
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> ClearCartAsync(int customerId, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));

        var items = await _context.CartItems
            .Where(item => item.Cart.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return 0;
        }

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync(cancellationToken);
        return items.Count;
    }

    private async Task<Product> GetPurchasableProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.ProductId == productId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {productId} was not found.");

        EnsurePurchasable(product);
        return product;
    }

    private static void EnsurePurchasable(Product product)
    {
        if (!product.IsActive)
        {
            throw new InvalidOperationException($"Product {product.ProductId} is inactive.");
        }

        if (!product.Category.IsActive)
        {
            throw new InvalidOperationException(
                $"Category {product.CategoryId} for product {product.ProductId} is inactive.");
        }
    }

    private static void EnsureStock(Product product, int requestedQuantity)
    {
        if (requestedQuantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                $"Requested quantity {requestedQuantity} exceeds stock {product.StockQuantity} for product {product.ProductId}.");
        }
    }

    private static int CheckedAdd(int currentQuantity, int quantityToAdd)
    {
        try
        {
            return checked(currentQuantity + quantityToAdd);
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityToAdd),
                "The resulting cart quantity is too large.");
        }
    }

    private static void ValidateIdentifiersAndQuantity(int customerId, int productId, int quantity)
    {
        ServiceValidation.RequirePositiveId(customerId, nameof(customerId));
        ServiceValidation.RequirePositiveId(productId, nameof(productId));

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }
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
