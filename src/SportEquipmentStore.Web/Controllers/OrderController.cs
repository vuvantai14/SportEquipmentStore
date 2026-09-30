using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Web.Extensions;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

[Authorize(Roles = "Customer")]
[Route("orders")]
public sealed class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!User.TryGetCustomerId(out var customerId))
        {
            return Forbid();
        }

        var orders = await _orderService.GetByCustomerAsync(customerId, cancellationToken);
        var viewModel = new OrderHistoryViewModel
        {
            Orders = orders
                .OrderByDescending(order => order.OrderDate)
                .Select(MapHistoryItem)
                .ToList(),
        };

        return View(viewModel);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(
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

        return View(MapDetail(order));
    }

    [HttpPost("{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
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

        if (!MapHistoryItem(order).CanCancel)
        {
            TempData["OrderError"] = "Đơn hàng không thể hủy ở trạng thái hiện tại.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        try
        {
            await _orderService.CancelByCustomerAsync(id, customerId, cancellationToken);
            TempData["OrderSuccess"] = "Đơn hàng đã được hủy và tồn kho đã được cập nhật.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            TempData["OrderError"] = "Đơn hàng không thể hủy ở trạng thái hiện tại.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["OrderError"] = "Đơn hàng vừa được cập nhật. Vui lòng kiểm tra lại trạng thái.";
        }
        catch (DbUpdateException)
        {
            TempData["OrderError"] = "Không thể cập nhật đơn hàng lúc này. Vui lòng thử lại sau.";
        }
        catch (OverflowException)
        {
            TempData["OrderError"] = "Không thể hoàn tất yêu cầu hủy đơn. Vui lòng thử lại sau.";
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    private static OrderHistoryItemViewModel MapHistoryItem(Order order)
    {
        return new OrderHistoryItemViewModel
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            TotalItems = order.OrderDetails.Sum(detail => detail.Quantity),
        };
    }

    private static CustomerOrderDetailViewModel MapDetail(Order order)
    {
        return new CustomerOrderDetailViewModel
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingFullName = order.ShippingFullName,
            ShippingPhone = order.ShippingPhone,
            ShippingAddress = order.ShippingAddress,
            Note = order.Note,
            Items = order.OrderDetails
                .OrderBy(detail => detail.OrderDetailId)
                .Select(detail => new CustomerOrderDetailItemViewModel
                {
                    ProductId = detail.ProductId,
                    ProductName = detail.ProductNameAtPurchase,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                })
                .ToList(),
        };
    }
}
