namespace SportEquipmentStore.Core.Models.Orders;

public sealed class CreateOrderRequest
{
    public int CustomerId { get; init; }

    public Guid CheckoutRequestId { get; init; }

    public string ShippingFullName { get; init; } = string.Empty;

    public string ShippingPhone { get; init; } = string.Empty;

    public string ShippingAddress { get; init; } = string.Empty;

    public string? Note { get; init; }
}
