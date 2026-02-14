using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenExpenseApp.Interfaces;
using OpenExpenseApp.Models;
using OpenExpenseApp.Utils;

namespace OpenExpenseApp.Controllers;

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
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        Console.WriteLine($"[DEBUG] Return URL: {returnUrl}");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string amount,
        DateTime expenseDate,
        string? description,
        string paymentMethod,
        string? returnUrl
    )
    {
        // Session sudah di-validate oleh middleware
        var userId = HttpContext.Session.GetString("UserId")!;
        Console.WriteLine($"[DEBUG] Return URL: {returnUrl}");

        // Parse amount dengan InvariantCulture untuk menghindari masalah kultur
        if (
            !decimal.TryParse(
                amount,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsedAmount
            )
        )
        {
            SweetAlert.Error(TempData, "Input Gagal", "Amount tidak valid");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        if (parsedAmount <= 0)
        {
            SweetAlert.Error(TempData, "Input Gagal", "Amount harus lebih dari 0");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            SweetAlert.Error(TempData, "Input Gagal", "Payment method harus diisi");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var expense = new Expense
        {
            Id = Ulid.NewUlid().ToString(),
            Amount = parsedAmount,
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
            $"Expense dengan amount {parsedAmount} berhasil ditambahkan untuk user {userId}"
        );
        SweetAlert.Success(TempData, "Pembuatan Berhasil", "Expense berhasil ditambahkan.");
        // Gunakan LocalRedirect untuk URL mentah (path) agar aman dari Open Redirect Attack
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
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
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlert.Error(TempData, "Tidak Ditemukan", "Expense tidak ditemukan");
            return RedirectToAction("Index");
        }

        return View(expense);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        string amount,
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
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlert.Error(TempData, "Tidak Ditemukan", "Expense tidak ditemukan");
            return RedirectToAction("Index");
        }

        // Parse amount dengan InvariantCulture untuk menghindari masalah kultur
        if (
            !decimal.TryParse(
                amount,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsedAmount
            )
        )
        {
            SweetAlert.Error(TempData, "Input Gagal", "Amount tidak valid");
            return View(expense);
        }

        if (parsedAmount <= 0)
        {
            SweetAlert.Error(TempData, "Input Gagal", "Amount harus lebih dari 0");
            return View(expense);
        }

        expense.Amount = parsedAmount;
        expense.ExpenseDate = expenseDate;
        expense.Description = description;
        expense.PaymentMethod = paymentMethod;

        await _expenseRepository.UpdateAsync(expense);

        _logger.LogInformation($"Expense {id} berhasil diupdate oleh user {userId}");
        SweetAlert.Success(TempData, "Update Berhasil", "Expense berhasil diupdate.");
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
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke expense ini"
            );
            return RedirectToAction("Index");
        }

        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            SweetAlert.Error(
                TempData,
                "Tidak Ditemukan",
                "Expense tidak ditemukan atau sudah dihapus."
            );
            return RedirectToAction("Index");
        }

        try
        {
            // Delete userExpense first, then expense (avoid cascade delete conflict)
            await _userExpenseRepository.DeleteAsync(userExpense);
            await _expenseRepository.DeleteAsync(expense);

            _logger.LogInformation($"Expense {id} berhasil dihapus oleh user {userId}");
            SweetAlert.Success(TempData, "Hapus Berhasil", "Expense berhasil dihapus.");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Entity mungkin sudah dihapus, coba refresh dan cek
            _logger.LogWarning($"Concurrency error saat menghapus expense {id} oleh user {userId}");

            // Cek lagi apakah expense sudah dihapus
            var expenseStillExists = await _expenseRepository.AnyAsync(e => e.Id == id);
            if (!expenseStillExists)
            {
                SweetAlert.Success(
                    TempData,
                    "Sudah Dihapus",
                    "Expense ini sudah dihapus sebelumnya."
                );
            }
            else
            {
                SweetAlert.Error(
                    TempData,
                    "Gagal",
                    "Expense tidak dapat dihapus. Silakan coba lagi."
                );
            }
        }

        return RedirectToAction("Index");
    }
}
