using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Views.Shared.Components;

/// <summary>
/// Reusable data card component with table
/// Usage: await Component.InvokeAsync("DataCard", new { title, iconClass, items, emptyMessage })
/// </summary>
public class DataCardViewComponent : ViewComponent
{
    public IViewComponentResult InvokeAsync(
        string title,
        string iconClass,
        IEnumerable<dynamic> items,
        string emptyMessage = "No data available"
    )
    {
        return View(
            new DataCardViewModel
            {
                Title = title,
                IconClass = iconClass,
                Items = items,
                EmptyMessage = emptyMessage,
            }
        );
    }
}

public class DataCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public IEnumerable<dynamic> Items { get; set; } = Enumerable.Empty<dynamic>();
    public string EmptyMessage { get; set; } = "No data available";
}
