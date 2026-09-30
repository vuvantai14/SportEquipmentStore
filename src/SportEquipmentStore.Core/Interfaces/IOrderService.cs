using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Enums;
using SportEquipmentStore.Core.Models.Orders;

namespace SportEquipmentStore.Core.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<Order> CreateOrderFromCartAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<Order> UpdateStatusAsync(
        int orderId,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default);

    Task<Order> CancelByCustomerAsync(
        int orderId,
        int customerId,
        CancellationToken cancellationToken = default);
}
