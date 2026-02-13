using Microsoft.AspNetCore.Mvc;

namespace OpenExpenseApp.Views.Shared.Components;

/// <summary>
/// Reusable empty state component
/// Usage: await Component.InvokeAsync("EmptyState", new { iconClass, title, message, actionUrl, actionText, actionCssClass })
/// </summary>
public class EmptyStateViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        string iconClass = "bi bi-inbox",
        string title = "No data",
        string message = "Start by adding your first item",
        string? actionUrl = null,
        string actionText = "Add New",
        string actionCssClass = "btn-primary"
    )
    {
        var model = new EmptyStateViewModel
        {
            IconClass = iconClass,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            ActionText = actionText,
            ActionCssClass = actionCssClass,
        };

        return await Task.FromResult(View(model));
    }
}

public class EmptyStateViewModel
{
    public string IconClass { get; set; } = "bi bi-inbox";
    public string Title { get; set; } = "No data";
    public string Message { get; set; } = "Start by adding your first item";
    public string? ActionUrl { get; set; }
    public string ActionText { get; set; } = "Add New";
    public string ActionCssClass { get; set; } = "btn-primary";
}
