using Microsoft.AspNetCore.Mvc;
using OpenExpenseApp.Interfaces;
using OpenExpenseApp.Models; // Pastikan namespace model benar

namespace OpenExpenseApp.Services;

// Tambahkan Route agar bisa diakses via /DashboardAPI/GetDashboardData
[Route("[controller]/[action]")]
public class DashboardAPI : Controller
{
    private readonly ILogger<DashboardAPI> _logger;
    private readonly IUserExpenseRepository _userExpenseRepository;

    public DashboardAPI(ILogger<DashboardAPI> logger, IUserExpenseRepository userExpenseRepository)
    {
        _logger = logger;
        _userExpenseRepository = userExpenseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var userExpenses = await _userExpenseRepository.GetByUserIdAsync(userId);
        var expenses =
            userExpenses?.Select(ue => ue.Expense).OfType<Expense>().ToList()
            ?? new List<Expense>();

        // 1. Hitung variabel yang error tadi
        var totalExpenses = expenses.Sum(e => e.Amount);

        var todayExpensesSum = expenses.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);

        var thisMonthExpenses = expenses
            .Where(e => e.ExpenseDate >= startOfMonth && e.ExpenseDate <= endOfMonth)
            .Sum(e => e.Amount);

        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        var avgPerDay = daysInMonth > 0 ? thisMonthExpenses / daysInMonth : 0;

        // 2. Hitung Grouping Payment Method
        var expensesByPaymentMethod = expenses
            .GroupBy(e => e.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var expensesByPaymentMethodPercent = expensesByPaymentMethod.ToDictionary(
            kvp => kvp.Key,
            kvp => totalExpenses > 0 ? (double)(kvp.Value / totalExpenses * 100) : 0
        );

        // 3. Masukkan ke ViewModel
        var data = new DashboardViewModel
        {
            TotalExpenses = totalExpenses,
            TodayExpenses = todayExpensesSum,
            ThisMonthExpenses = thisMonthExpenses,
            AvgPerDay = avgPerDay,
            TotalExpenseCount = expenses.Count,

            RecentExpenses = expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(10)
                .Select(e => new Expense
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    PaymentMethod = e.PaymentMethod,
                    // Relasi navigasi (UserExpenses) dibiarkan kosong untuk memutus Circular Reference
                })
                .ToList(),

            ExpensesByPaymentMethod = expensesByPaymentMethod,
            ExpensesByPaymentMethodPercent = expensesByPaymentMethodPercent,
        };

        return Json(data);
    }
}
