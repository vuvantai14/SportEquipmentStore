using SportEquipmentStore.Core.Entities;

namespace SportEquipmentStore.Core.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);

    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);

    Task<Category> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
