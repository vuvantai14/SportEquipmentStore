using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Core.Models.Orders;
using SportEquipmentStore.Web.Extensions;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

[Authorize(Roles = "Customer")]
[Route("checkout")]
public sealed class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    public CheckoutController(
        ICartService cartService,
        ICustomerService customerService,
        IOrderService orderService)
    {
        _cartService = cartService;
        _customerService = customerService;
        _orderService = orderService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var customer = await _customerService.GetByIdAsync(
            customerId,
            cancellationToken);
        if (customer is null)
        {
            return Forbid();
        }

        var items = await GetCheckoutItemsAsync(customerId, cancellationToken);
        if (items.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng của bạn đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        var viewModel = new CheckoutViewModel
        {
            ShippingFullName = customer.User.FullName,
            ShippingPhone = customer.Phone ?? string.Empty,
            ShippingAddress = customer.Address ?? string.Empty,
            CheckoutRequestId = Guid.NewGuid(),
            Items = items,
        };

        return View(viewModel);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        CheckoutViewModel model,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        if (model.CheckoutRequestId == Guid.Empty)
        {
            ModelState.AddModelError(
                string.Empty,
                "Phiên đặt hàng không hợp lệ. Vui lòng tải lại trang.");
        }

        if (!ModelState.IsValid)
        {
            if (!await PopulateItemsAsync(model, customerId, cancellationToken))
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }

        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(
                new CreateOrderRequest
                {
                    CustomerId = customerId,
                    CheckoutRequestId = model.CheckoutRequestId,
                    ShippingFullName = model.ShippingFullName,
                    ShippingPhone = model.ShippingPhone,
                    ShippingAddress = model.ShippingAddress,
                    Note = model.Note,
                },
                cancellationToken);

            return RedirectToAction(nameof(Success), new { id = order.OrderId });
        }
        catch (Exception exception) when (IsExpectedCheckoutException(exception))
        {
            ModelState.AddModelError(string.Empty, GetCheckoutError(exception));

            if (!await PopulateItemsAsync(model, customerId, cancellationToken))
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }
    }

    [HttpGet("success/{id:int}")]
    public async Task<IActionResult> Success(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId) || id <= 0)
        {
            return NotFound();
        }

        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null || order.CustomerId != customerId)
        {
            return NotFound();
        }

        var viewModel = new OrderSuccessViewModel
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingFullName = order.ShippingFullName,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            Items = order.OrderDetails
                .OrderBy(detail => detail.OrderDetailId)
                .Select(detail => new CheckoutItemViewModel
                {
                    ProductName = detail.ProductNameAtPurchase,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                })
                .ToList(),
        };

        return View(viewModel);
    }

    private async Task<bool> PopulateItemsAsync(
        CheckoutViewModel model,
        int customerId,
        CancellationToken cancellationToken)
    {
        model.Items = await GetCheckoutItemsAsync(customerId, cancellationToken);
        if (model.Items.Count > 0)
        {
            return true;
        }

        TempData["CartError"] = "Giỏ hàng của bạn đang trống.";
        return false;
    }

    private async Task<IReadOnlyList<CheckoutItemViewModel>> GetCheckoutItemsAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartByCustomerAsync(
            customerId,
            cancellationToken);

        if (cart is null)
        {
            return [];
        }

        return cart.CartItems
            .OrderBy(item => item.AddedAt)
            .Select(item => new CheckoutItemViewModel
            {
                ProductName = item.Product.ProductName,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
            })
            .ToList();
    }

    private static bool IsExpectedCheckoutException(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or KeyNotFoundException
            or OverflowException
            or DbUpdateException;
    }

    private static string GetCheckoutError(Exception exception)
    {
        return exception switch
        {
            DbUpdateConcurrencyException =>
                "Tồn kho vừa thay đổi. Vui lòng kiểm tra lại giỏ hàng.",
            InvalidOperationException =>
                "Không thể đặt hàng. Sản phẩm có thể đã hết hàng hoặc không còn khả dụng.",
            KeyNotFoundException =>
                "Không thể đặt hàng vì thông tin khách hàng hoặc sản phẩm không còn tồn tại.",
            _ => "Không thể hoàn tất đơn hàng. Vui lòng kiểm tra thông tin và thử lại.",
        };
    }
}
