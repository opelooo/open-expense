using AccountingApp.Helpers;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using AccountingApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Controllers;

public class ExpenseController : Controller
{
    private readonly ILogger<ExpenseController> _logger;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUserExpenseRepository _userExpenseRepository;

    public ExpenseController(
        ILogger<ExpenseController> logger,
        IExpenseRepository expenseRepository,
        IUserExpenseRepository userExpenseRepository
    )
    {
        _logger = logger;
        _expenseRepository = expenseRepository;
        _userExpenseRepository = userExpenseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        // Panggil fungsi repository yang baru
        var (items, totalItems) = await _userExpenseRepository.GetByUserIdPaginatedAsync(
            userId,
            page,
            pageSize
        );

        var paginationModel = new PaginationModel<Expense>
        {
            Items = items.Select(ue => ue.Expense).OfType<Expense>().ToList(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
        };

        return View(paginationModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        decimal amount,
        DateTime expenseDate,
        string? description,
        string paymentMethod
    )
    {
        // Session sudah di-validate oleh middleware
        var userId = HttpContext.Session.GetString("UserId")!;

        if (amount <= 0)
        {
            SweetAlertHelper.Error(TempData, "Input Gagal", "Amount harus lebih dari 0");
            return View();
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            SweetAlertHelper.Error(TempData, "Input Gagal", "Payment method harus diisi");
            return View();
        }

        var expense = new Expense
        {
            Id = Ulid.NewUlid().ToString(),
            Amount = amount,
            ExpenseDate = expenseDate,
            Description = description,
            PaymentMethod = paymentMethod,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _expenseRepository.CreateAsync(expense);

        // Create UserExpense link
        var userExpense = new UserExpense
        {
            Id = Ulid.NewUlid().ToString(),
            UserId = userId,
            ExpenseId = expense.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _userExpenseRepository.CreateAsync(userExpense);

        _logger.LogInformation(
            $"Expense dengan amount {amount} berhasil ditambahkan untuk user {userId}"
        );
        SweetAlertHelper.Success(TempData, "Expense Berhasil", "Expense berhasil ditambahkan.");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        // Session sudah di-validate oleh middleware
        var userId = HttpContext.Session.GetString("UserId")!;

        // Check if expense belongs to current user
        var userExpense = await _userExpenseRepository.FirstOrDefaultAsync(ue =>
            ue.ExpenseId == id && ue.UserId == userId
        );
        if (userExpense == null)
        {
            SweetAlertHelper.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlertHelper.Error(TempData, "Tidak Ditemukan", "Expense tidak ditemukan");
            return RedirectToAction("Index");
        }

        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        decimal amount,
        DateTime expenseDate,
        string? description,
        string paymentMethod
    )
    {
        // Session sudah di-validate oleh middleware
        var userId = HttpContext.Session.GetString("UserId")!;

        // Check if expense belongs to current user
        var userExpense = await _userExpenseRepository.FirstOrDefaultAsync(ue =>
            ue.ExpenseId == id && ue.UserId == userId
        );
        if (userExpense == null)
        {
            SweetAlertHelper.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlertHelper.Error(TempData, "Tidak Ditemukan", "Expense tidak ditemukan");
            return RedirectToAction("Index");
        }

        if (amount <= 0)
        {
            SweetAlertHelper.Error(TempData, "Input Gagal", "Amount harus lebih dari 0");
            return View(expense);
        }

        expense.Amount = amount;
        expense.ExpenseDate = expenseDate;
        expense.Description = description;
        expense.PaymentMethod = paymentMethod;

        await _expenseRepository.UpdateAsync(expense);

        _logger.LogInformation($"Expense {id} berhasil diupdate oleh user {userId}");
        SweetAlertHelper.Success(TempData, "Update Berhasil", "Expense berhasil diupdate.");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        // Session sudah di-validate oleh middleware
        var userId = HttpContext.Session.GetString("UserId")!;

        // Check if expense belongs to current user
        var userExpense = await _userExpenseRepository.FirstOrDefaultAsync(ue =>
            ue.ExpenseId == id && ue.UserId == userId
        );
        if (userExpense == null)
        {
            SweetAlertHelper.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlertHelper.Error(TempData, "Tidak Ditemukan", "Expense tidak ditemukan");
            return RedirectToAction("Index");
        }

        // Delete the expense (UserExpense will be deleted due to cascade delete)
        await _expenseRepository.DeleteAsync(expense);

        // Also delete the user expense record explicitly if cascade isn't working
        await _userExpenseRepository.DeleteAsync(userExpense);

        _logger.LogInformation($"Expense {id} berhasil dihapus oleh user {userId}");
        SweetAlertHelper.Success(TempData, "Hapus Berhasil", "Expense berhasil dihapus.");
        return RedirectToAction("Index");
    }
}
