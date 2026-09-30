using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SportEquipmentStore.Core.Interfaces;
using SportEquipmentStore.Web.Models;

namespace SportEquipmentStore.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;

    public HomeController(
        ILogger<HomeController> logger,
        ICategoryService categoryService,
        IProductService productService)
    {
        _logger = logger;
        _categoryService = categoryService;
        _productService = productService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetActiveAsync(cancellationToken);
        var products = await _productService.GetActiveAsync(cancellationToken);

        var viewModel = new HomeViewModel
        {
            FeaturedCategories = categories
                .Take(6)
                .Select(category => new HomeCategoryViewModel
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description
                })
                .ToArray(),
            FeaturedProducts = products
                .Take(8)
                .Select(product => new ProductCardViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    CategoryName = product.Category.CategoryName,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    ImageUrl = product.ImageUrl
                })
                .ToArray()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
