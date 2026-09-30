using SportEquipmentStore.Core.Enums;

namespace SportEquipmentStore.Web.Models;

public sealed class OrderSuccessViewModel
{
    public int OrderId { get; init; }

    public DateTime OrderDate { get; init; }

    public decimal TotalAmount { get; init; }

    public OrderStatus Status { get; init; }

    public string ShippingFullName { get; init; } = string.Empty;

    public string ShippingPhone { get; init; } = string.Empty;

    public string ShippingAddress { get; init; } = string.Empty;

    public IReadOnlyList<CheckoutItemViewModel> Items { get; init; } = [];

    public int TotalItems => Items.Sum(item => item.Quantity);
}
