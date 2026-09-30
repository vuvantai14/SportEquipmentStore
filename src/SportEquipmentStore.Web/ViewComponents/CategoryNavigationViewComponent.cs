using Microsoft.AspNetCore.Mvc;
using SportEquipmentStore.Core.Interfaces;

namespace SportEquipmentStore.Web.ViewComponents;

public sealed class CategoryNavigationViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;

    public CategoryNavigationViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _categoryService.GetActiveAsync(HttpContext.RequestAborted);
        return View(categories);
    }
}
