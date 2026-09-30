using SportEquipmentStore.Core.Entities;

namespace SportEquipmentStore.Core.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Customer?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Customer> CreateAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<Customer> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}
