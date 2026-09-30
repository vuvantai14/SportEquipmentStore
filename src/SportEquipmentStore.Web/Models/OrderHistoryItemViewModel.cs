using SportEquipmentStore.Core.Enums;

namespace SportEquipmentStore.Web.Models;

public sealed class OrderHistoryItemViewModel
{
    public int OrderId { get; init; }

    public DateTime OrderDate { get; init; }

    public OrderStatus Status { get; init; }

    public decimal TotalAmount { get; init; }

    public int TotalItems { get; init; }

    public bool CanCancel => Status == OrderStatus.Pending;
}
