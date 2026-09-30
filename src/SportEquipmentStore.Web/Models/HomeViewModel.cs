namespace SportEquipmentStore.Web.Models;

public sealed class HomeViewModel
{
    public IReadOnlyList<HomeCategoryViewModel> FeaturedCategories { get; init; } =
        Array.Empty<HomeCategoryViewModel>();

    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; init; } =
        Array.Empty<ProductCardViewModel>();
}
