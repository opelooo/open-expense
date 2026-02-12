using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Views.Shared.Components;

/// <summary>
/// Reusable quick actions card component
/// Usage: await Component.InvokeAsync("QuickActions", new { actions })
/// </summary>
public class QuickActionsCardViewComponent : ViewComponent
{
    public IViewComponentResult InvokeAsync(IEnumerable<QuickActionItem> actions)
    {
        return View(new QuickActionsCardViewModel { Actions = actions });
    }
}

public class QuickActionsCardViewModel
{
    public IEnumerable<QuickActionItem> Actions { get; set; } = Enumerable.Empty<QuickActionItem>();
}

public class QuickActionItem
{
    public string Text { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
    public string CssClass { get; set; } = "btn-outline-secondary";
}
