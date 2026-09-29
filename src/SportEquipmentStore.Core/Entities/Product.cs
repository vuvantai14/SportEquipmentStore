namespace SportEquipmentStore.Core.Entities;

public class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category Category { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
