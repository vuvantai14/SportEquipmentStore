namespace SportEquipmentStore.Web.Models;

public sealed class OrderHistoryViewModel
{
    public IReadOnlyList<OrderHistoryItemViewModel> Orders { get; init; } = [];
}
