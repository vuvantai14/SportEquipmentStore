namespace SportEquipmentStore.Web.Models;

public sealed class CartItemViewModel
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public int StockQuantity { get; init; }

    public bool CanUpdate { get; init; }

    public decimal LineTotal => UnitPrice * Quantity;
}
