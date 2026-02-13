using Microsoft.AspNetCore.Mvc;

namespace OpenExpenseApp.Views.Shared.Components;

/// <summary>
/// Reusable page header component
/// Usage: await Component.InvokeAsync("PageHeader", new { title, subtitle })
/// </summary>
public class PageHeaderViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        string title,
        string? subtitle = null,
        bool isLoading = false
    )
    {
        var model = new PageHeaderViewModel { Title = title, Subtitle = subtitle };

        return await Task.FromResult(View(model));
    }
}

public class PageHeaderViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
}
