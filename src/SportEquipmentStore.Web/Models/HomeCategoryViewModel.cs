namespace SportEquipmentStore.Web.Models;

public sealed class HomeCategoryViewModel
{
    public int CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }
}
