using Microsoft.AspNetCore.Mvc;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Web.Extensions;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.ViewComponents;

public sealed class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartSummaryViewComponent(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true
            || !UserClaimsPrincipal.TryGetCustomerId(out var customerId))
        {
            return View(new CartBadgeViewModel());
        }

        var cart = await _cartService.GetCartByCustomerAsync(
            customerId,
            HttpContext.RequestAborted);
        var totalQuantity = cart?.CartItems.Sum(item => item.Quantity) ?? 0;

        return View(new CartBadgeViewModel
        {
            IsAuthenticated = true,
            TotalQuantity = totalQuantity,
        });
    }
}
