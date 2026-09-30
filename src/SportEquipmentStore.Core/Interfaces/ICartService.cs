using SportEquipmentStore.Core.Entities;

namespace SportEquipmentStore.Core.Interfaces;

public interface ICartService
{
    Task<Cart?> GetCartByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    Task<CartItem> AddItemAsync(
        int customerId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<CartItem> UpdateQuantityAsync(
        int customerId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveItemAsync(
        int customerId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<int> ClearCartAsync(int customerId, CancellationToken cancellationToken = default);
}
