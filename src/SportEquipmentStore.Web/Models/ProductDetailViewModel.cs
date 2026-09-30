namespace SportEquipmentStore.Web.Models;

public sealed class ProductDetailViewModel
{
    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public string? ImageUrl { get; init; }
}
