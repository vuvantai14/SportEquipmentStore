namespace SportEquipmentStore.Web.Models;

public sealed class ProductCardViewModel
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public string? ImageUrl { get; init; }

    public string? DetailUrl { get; init; }
}
