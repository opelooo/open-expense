using Microsoft.AspNetCore.Mvc;

namespace OpenExpenseApp.Views.Shared.Components;

/// <summary>
/// Reusable page actions component (filter bar with pagination and action buttons)
/// Usage: await Component.InvokeAsync("PageActions", new { pageSize, pageSizes, createUrl, createText, createCssClass })
/// </summary>
public class PageActionsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        int pageSize,
        int[] pageSizes,
        string createUrl,
        string createText = "Tambah",
        string createCssClass = "btn-primary",
        string returnUrl = ""
    )
    {
        var model = new PageActionsViewModel
        {
            PageSize = pageSize,
            PageSizes = pageSizes,
            CreateUrl = createUrl,
            CreateText = createText,
            CreateCssClass = createCssClass,
            ReturnUrl = returnUrl,
        };

        return await Task.FromResult(View(model));
    }
}

public class PageActionsViewModel
{
    public int PageSize { get; set; }
    public int[] PageSizes { get; set; } = { 5, 10, 25, 50 };
    public string CreateUrl { get; set; } = string.Empty;
    public string CreateText { get; set; } = "Tambah";
    public string CreateCssClass { get; set; } = "btn-primary";
    public string ReturnUrl { get; set; } = string.Empty;
}
