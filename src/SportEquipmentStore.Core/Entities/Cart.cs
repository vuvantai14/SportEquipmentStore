namespace SportEquipmentStore.Core.Entities;

public class Cart
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
