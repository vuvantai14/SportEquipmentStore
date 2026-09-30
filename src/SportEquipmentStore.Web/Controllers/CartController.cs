using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Web.Extensions;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

[Authorize(Roles = "Customer")]
[Route("cart")]
public sealed class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var cart = await _cartService.GetCartByCustomerAsync(
            customerId,
            cancellationToken);
        var viewModel = new CartViewModel
        {
            Items = cart is null
                ? []
                : cart.CartItems
                .OrderBy(item => item.AddedAt)
                .Select(item => new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    ImageUrl = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    StockQuantity = item.Product.StockQuantity,
                    CanUpdate = item.Product.IsActive
                        && item.Product.Category.IsActive
                        && item.Product.StockQuantity > 0,
                })
                .ToList(),
        };

        return View(viewModel);
    }

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        CartItemInputModel model,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Sản phẩm hoặc số lượng không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cartService.AddItemAsync(
                customerId,
                model.ProductId,
                model.Quantity,
                cancellationToken);
            TempData["CartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng.";
        }
        catch (Exception exception) when (IsExpectedCartException(exception))
        {
            SetCartError(exception);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        CartItemInputModel model,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["CartError"] = "Số lượng cập nhật không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cartService.UpdateQuantityAsync(
                customerId,
                model.ProductId,
                model.Quantity,
                cancellationToken);
            TempData["CartSuccess"] = "Đã cập nhật giỏ hàng.";
        }
        catch (Exception exception) when (IsExpectedCartException(exception))
        {
            SetCartError(exception);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(
        int productId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        if (productId <= 0)
        {
            TempData["CartError"] = "Sản phẩm không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var removed = await _cartService.RemoveItemAsync(
            customerId,
            productId,
            cancellationToken);
        TempData[removed ? "CartSuccess" : "CartError"] = removed
            ? "Đã xóa sản phẩm khỏi giỏ hàng."
            : "Sản phẩm không còn trong giỏ hàng.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var removedItems = await _cartService.ClearCartAsync(
            customerId,
            cancellationToken);
        TempData[removedItems > 0 ? "CartSuccess" : "CartError"] = removedItems > 0
            ? "Đã xóa toàn bộ giỏ hàng."
            : "Giỏ hàng của bạn đã trống.";

        return RedirectToAction(nameof(Index));
    }

    private static bool IsExpectedCartException(Exception exception)
    {
        return exception is ArgumentException
            or InvalidOperationException
            or KeyNotFoundException
            or DbUpdateConcurrencyException;
    }

    private void SetCartError(Exception exception)
    {
        TempData["CartError"] = exception switch
        {
            ArgumentException => "Sản phẩm hoặc số lượng không hợp lệ.",
            KeyNotFoundException => "Sản phẩm không tồn tại hoặc không còn trong giỏ hàng.",
            DbUpdateConcurrencyException => "Tồn kho vừa thay đổi. Vui lòng kiểm tra và thử lại.",
            _ => "Sản phẩm không còn khả dụng hoặc số lượng vượt quá tồn kho.",
        };
    }
}
