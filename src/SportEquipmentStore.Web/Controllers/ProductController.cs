using Microsoft.AspNetCore.Mvc;
using SportEquipmentStore.Core.Entities;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

[Route("products")]
public sealed class ProductController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;

    public ProductController(
        ICategoryService categoryService,
        IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int? categoryId,
        string? sort,
        string? q,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetActiveAsync(cancellationToken);
        var categoryWasRequested = !string.IsNullOrWhiteSpace(
            Request.Query["categoryId"].ToString());
        var selectedCategoryId = categoryId is > 0
            && categories.Any(category => category.CategoryId == categoryId.Value)
                ? categoryId
                : null;

        var searchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        IReadOnlyList<Product> products = searchQuery is null
            ? await _productService.GetActiveAsync(cancellationToken)
            : await _productService.SearchAsync(searchQuery, cancellationToken);

        IEnumerable<Product> filteredProducts = products;
        if (selectedCategoryId.HasValue)
        {
            filteredProducts = filteredProducts.Where(
                product => product.CategoryId == selectedCategoryId.Value);
        }

        var selectedSort = NormalizeSort(sort);
        var sortedProducts = SortProducts(filteredProducts, selectedSort);

        var viewModel = new ProductCatalogViewModel
        {
            Categories = categories
                .Select(category => new ProductCategoryFilterViewModel
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                })
                .ToList(),
            Products = sortedProducts
                .Select(product => new ProductCardViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    CategoryName = product.Category.CategoryName,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    ImageUrl = product.ImageUrl,
                })
                .ToList(),
            SelectedCategoryId = selectedCategoryId,
            SelectedSort = selectedSort,
            SearchQuery = searchQuery,
            FilterNotice = categoryWasRequested && !selectedCategoryId.HasValue
                ? "Bộ lọc danh mục không hợp lệ và đã được bỏ qua."
                : null,
        };

        return View(viewModel);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product is null || !product.IsActive || !product.Category.IsActive)
        {
            return NotFound();
        }

        var viewModel = new ProductDetailViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            CategoryName = product.Category.CategoryName,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
        };

        return View(viewModel);
    }

    private static string NormalizeSort(string? sort)
    {
        return sort?.Trim().ToLowerInvariant() switch
        {
            "price-asc" => "price-asc",
            "price-desc" => "price-desc",
            "name-asc" => "name-asc",
            _ => "default",
        };
    }

    private static IEnumerable<Product> SortProducts(
        IEnumerable<Product> products,
        string sort)
    {
        return sort switch
        {
            "price-asc" => products
                .OrderBy(product => product.Price)
                .ThenBy(product => product.ProductName)
                .ThenBy(product => product.ProductId),
            "price-desc" => products
                .OrderByDescending(product => product.Price)
                .ThenBy(product => product.ProductName)
                .ThenBy(product => product.ProductId),
            "name-asc" => products
                .OrderBy(product => product.ProductName)
                .ThenBy(product => product.ProductId),
            _ => products
                .OrderBy(product => product.ProductName)
                .ThenBy(product => product.ProductId),
        };
    }
}
