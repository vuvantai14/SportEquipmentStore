namespace SportEquipmentStore.Web.Models;

public sealed class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; init; } = [];

    public int TotalItems => Items.Sum(item => item.Quantity);

    public decimal TotalAmount => Items.Sum(item => item.LineTotal);
}
