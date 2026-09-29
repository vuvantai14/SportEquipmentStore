namespace SportEquipmentStore.Core.Entities;

public class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductNameAtPurchase { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
