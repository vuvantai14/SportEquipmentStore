using SportEquipmentStore.Core.Entities;

namespace SportEquipmentStore.Core.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchAsync(string? keyword, CancellationToken cancellationToken = default);

    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
