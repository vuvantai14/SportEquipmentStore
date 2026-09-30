using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly SportEquipmentStoreDbContext _context;

    public CategoryService(SportEquipmentStoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        return _context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(category => category.CategoryId == id, cancellationToken);
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (category.CategoryId != 0)
        {
            throw new ArgumentException("A new category cannot already have an identifier.", nameof(category));
        }

        Normalize(category);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        ServiceValidation.RequirePositiveId(category.CategoryId, nameof(category.CategoryId));
        Normalize(category);

        var existing = await _context.Categories
            .SingleOrDefaultAsync(item => item.CategoryId == category.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category {category.CategoryId} was not found.");

        existing.CategoryName = category.CategoryName;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<Category> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        var category = await _context.Categories
            .SingleOrDefaultAsync(item => item.CategoryId == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category {id} was not found.");

        category.IsActive = isActive;
        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    private static void Normalize(Category category)
    {
        category.CategoryName = ServiceValidation.RequiredText(
            category.CategoryName,
            120,
            nameof(category.CategoryName));
        category.Description = ServiceValidation.OptionalText(
            category.Description,
            1000,
            nameof(category.Description));
    }
}
