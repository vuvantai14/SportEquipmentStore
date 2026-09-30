using Microsoft.EntityFrameworkCore;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Data.Context;

namespace SportEquipmentStore.Data.Services;

public sealed class ProductService : IProductService
{
    private readonly SportEquipmentStoreDbContext _context;

    public ProductService(SportEquipmentStoreDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await ProductsWithCategory()
            .OrderBy(product => product.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await ActiveProducts()
            .OrderBy(product => product.ProductName)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        return ProductsWithCategory()
            .SingleOrDefaultAsync(product => product.ProductId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(categoryId, cancellationToken);

        return await ProductsWithCategory()
            .Where(product => product.CategoryId == categoryId)
            .OrderBy(product => product.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(categoryId, cancellationToken);

        return await ActiveProducts()
            .Where(product => product.CategoryId == categoryId)
            .OrderBy(product => product.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return await GetActiveAsync(cancellationToken);
        }

        var normalizedKeyword = keyword.Trim();
        return await ActiveProducts()
            .Where(product => product.ProductName.Contains(normalizedKeyword))
            .OrderBy(product => product.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (product.ProductId != 0)
        {
            throw new ArgumentException("A new product cannot already have an identifier.", nameof(product));
        }

        NormalizeAndValidate(product);
        var category = await GetTrackedCategoryAsync(product.CategoryId, cancellationToken);

        product.Category = category;
        if (product.CreatedAt == default)
        {
            product.CreatedAt = DateTime.UtcNow;
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        ServiceValidation.RequirePositiveId(product.ProductId, nameof(product.ProductId));
        NormalizeAndValidate(product);

        var category = await GetTrackedCategoryAsync(product.CategoryId, cancellationToken);
        var existing = await _context.Products
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.ProductId == product.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {product.ProductId} was not found.");

        if (product.RowVersion.Length > 0)
        {
            _context.Entry(existing).Property(item => item.RowVersion).OriginalValue = product.RowVersion;
        }

        existing.ProductName = product.ProductName;
        existing.CategoryId = product.CategoryId;
        existing.Category = category;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.Description = product.Description;
        existing.ImageUrl = product.ImageUrl;
        existing.IsActive = product.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<Product> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ServiceValidation.RequirePositiveId(id, nameof(id));

        var product = await _context.Products
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.ProductId == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {id} was not found.");

        product.IsActive = isActive;
        await _context.SaveChangesAsync(cancellationToken);

        return product;
    }

    private IQueryable<Product> ProductsWithCategory()
    {
        return _context.Products
            .AsNoTracking()
            .Include(product => product.Category);
    }

    private IQueryable<Product> ActiveProducts()
    {
        return ProductsWithCategory()
            .Where(product => product.IsActive && product.Category.IsActive);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        ServiceValidation.RequirePositiveId(categoryId, nameof(categoryId));

        if (!await _context.Categories.AnyAsync(
                category => category.CategoryId == categoryId,
                cancellationToken))
        {
            throw new KeyNotFoundException($"Category {categoryId} was not found.");
        }
    }

    private async Task<Category> GetTrackedCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        ServiceValidation.RequirePositiveId(categoryId, nameof(categoryId));

        return await _context.Categories
            .SingleOrDefaultAsync(category => category.CategoryId == categoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category {categoryId} was not found.");
    }

    private static void NormalizeAndValidate(Product product)
    {
        product.ProductName = ServiceValidation.RequiredText(
            product.ProductName,
            200,
            nameof(product.ProductName));
        product.Description = ServiceValidation.OptionalText(
            product.Description,
            2000,
            nameof(product.Description));
        product.ImageUrl = ServiceValidation.OptionalText(
            product.ImageUrl,
            2048,
            nameof(product.ImageUrl));

        if (product.Price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(product.Price), "Price cannot be negative.");
        }

        if (product.StockQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(product.StockQuantity),
                "Stock quantity cannot be negative.");
        }
    }
}
