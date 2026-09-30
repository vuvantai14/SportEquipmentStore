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
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetActiveAsync(cancellationToken);
        var categoryWasRequested = !string.IsNullOrWhiteSpace(
            Request.Query["categoryId"].ToString());
        var selectedCategoryId = categoryId is > 0
            && categories.Any(category => category.CategoryId == categoryId.Value)
                ? categoryId
                : null;

        IReadOnlyList<Product> products = selectedCategoryId.HasValue
            ? await _productService.GetActiveByCategoryAsync(
                selectedCategoryId.Value,
                cancellationToken)
            : await _productService.GetActiveAsync(cancellationToken);

        var selectedSort = NormalizeSort(sort);
        var sortedProducts = SortProducts(products, selectedSort);

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
                    DetailUrl = null,
                })
                .ToList(),
            SelectedCategoryId = selectedCategoryId,
            SelectedSort = selectedSort,
            FilterNotice = categoryWasRequested && !selectedCategoryId.HasValue
                ? "Danh mục không hợp lệ. Đang hiển thị tất cả sản phẩm."
                : null,
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
