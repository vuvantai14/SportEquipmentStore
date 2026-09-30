namespace SportEquipmentStore.Web.Models;

public sealed class CartBadgeViewModel
{
    public bool IsAuthenticated { get; init; }

    public int TotalQuantity { get; init; }
}
