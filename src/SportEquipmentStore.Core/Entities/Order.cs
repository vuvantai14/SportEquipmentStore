using SportEquipmentStore.Core.Enums;

namespace SportEquipmentStore.Core.Entities;

public class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public Guid CheckoutRequestId { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string ShippingFullName { get; set; } = string.Empty;

    public string ShippingPhone { get; set; } = string.Empty;

    public string ShippingAddress { get; set; } = string.Empty;

    public string? Note { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Customer Customer { get; set; } = null!;

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
