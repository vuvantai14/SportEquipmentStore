namespace SportEquipmentStore.Web.Models;

public sealed class ProductCatalogViewModel
{
    public IReadOnlyList<ProductCardViewModel> Products { get; init; } = [];

    public IReadOnlyList<ProductCategoryFilterViewModel> Categories { get; init; } = [];

    public int? SelectedCategoryId { get; init; }

    public string SelectedSort { get; init; } = "default";

    public string? SearchQuery { get; init; }

    public string? FilterNotice { get; init; }

    public int TotalProducts => Products.Count;
}
