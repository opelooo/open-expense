using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenExpenseApp.Interfaces;
using OpenExpenseApp.Models;

namespace OpenExpenseApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserExpenseRepository _userExpenseRepository;

    public HomeController(
        ILogger<HomeController> logger,
        IUserExpenseRepository userExpenseRepository
    )
    {
        _logger = logger;
        _userExpenseRepository = userExpenseRepository;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult RenderDashboardPartial([FromBody] System.Text.Json.JsonElement rawData)
    {
        try
        {
            // Menggunakan JsonElement + Deserialize manual untuk menghindari NullReference
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var model = System.Text.Json.JsonSerializer.Deserialize<DashboardViewModel>(
                rawData.GetRawText(),
                options
            );

            if (model == null)
                return BadRequest("Data model kosong");

            return PartialView("_DashboardContent", model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
