namespace SportEquipmentStore.Web.Models;

public sealed class CustomerOrderDetailItemViewModel
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public decimal LineTotal => UnitPrice * Quantity;
}
