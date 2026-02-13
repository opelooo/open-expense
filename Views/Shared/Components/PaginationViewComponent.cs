using Microsoft.AspNetCore.Mvc;

namespace OpenExpenseApp.Views.Shared.Components;

/// <summary>
/// Reusable pagination component
/// Usage: await Component.InvokeAsync("Pagination", new { model = yourPaginationModel, controller = "ControllerName" })
/// </summary>
public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(object model, string controller)
    {
        var modelWrapper = new PaginationViewModel { Model = model, Controller = controller };

        return View(modelWrapper);
    }
}

public class PaginationViewModel
{
    public object Model { get; set; } = null!;
    public string Controller { get; set; } = string.Empty;

    // Helper properties for accessing pagination data
    public int CurrentPage => (int)Model.GetType().GetProperty("CurrentPage")!.GetValue(Model)!;
    public int PageSize => (int)Model.GetType().GetProperty("PageSize")!.GetValue(Model)!;
    public int TotalItems => (int)Model.GetType().GetProperty("TotalItems")!.GetValue(Model)!;
    public int TotalPages => (int)Model.GetType().GetProperty("TotalPages")!.GetValue(Model)!;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int ItemsCount =>
        ((System.Collections.IEnumerable)Model.GetType().GetProperty("Items")!.GetValue(Model)!)
            .Cast<object>()
            .Count();
}
