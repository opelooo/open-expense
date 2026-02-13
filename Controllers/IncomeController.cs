using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenExpenseApp.Interfaces;
using OpenExpenseApp.Models;
using OpenExpenseApp.Utils;

namespace OpenExpenseApp.Controllers;

public class IncomeController : Controller
{
    private readonly ILogger<IncomeController> _logger;
    private readonly IIncomeRepository _incomeRepository;

    public IncomeController(ILogger<IncomeController> logger, IIncomeRepository incomeRepository)
    {
        _logger = logger;
        _incomeRepository = incomeRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        var incomes = await _incomeRepository.FindAsync(i => i.UserId == userId);
        var totalItems = incomes.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var paginatedIncomes = incomes
            .OrderByDescending(i => i.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var paginationModel = new PaginationModel<Income>
        {
            Items = paginatedIncomes,
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
        var model = new Income { ReceivedDate = DateTime.Today, IsTaxable = true };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string sourceName,
        string incomeType,
        string grossAmount,
        DateTime receivedDate,
        string? taxWithheld,
        bool? isTaxable,
        string? returnUrl
    )
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            SweetAlert.Error(TempData, "Input Gagal", "Source name harus diisi");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        if (string.IsNullOrWhiteSpace(incomeType))
        {
            SweetAlert.Error(TempData, "Input Gagal", "Income type harus diisi");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var parsedGrossAmount = decimal.Parse(grossAmount ?? "0", CultureInfo.InvariantCulture);
        if (parsedGrossAmount <= 0)
        {
            SweetAlert.Error(TempData, "Input Gagal", "Gross amount harus lebih dari 0");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var parsedTaxWithheld = !string.IsNullOrEmpty(taxWithheld)
            ? decimal.Parse(taxWithheld, CultureInfo.InvariantCulture)
            : 0;

        var income = new Income
        {
            Id = Ulid.NewUlid().ToString(),
            UserId = userId,
            SourceName = sourceName,
            IncomeType = incomeType,
            GrossAmount = parsedGrossAmount,
            ReceivedDate = receivedDate,
            TaxWithheld = parsedTaxWithheld,
            IsTaxable = isTaxable ?? true,
            CreatedAt = DateTime.UtcNow,
        };

        await _incomeRepository.CreateAsync(income);

        _logger.LogInformation(
            $"Income dengan source {sourceName} dan amount {grossAmount} berhasil ditambahkan untuk user {userId}"
        );
        SweetAlert.Success(TempData, "Pembuatan Berhasil", "Income berhasil ditambahkan.");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        var income = await _incomeRepository.FirstOrDefaultAsync(i =>
            i.Id == id && i.UserId == userId
        );
        if (income == null)
        {
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke income ini"
            );
            return RedirectToAction("Index");
        }

        return View(income);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        string sourceName,
        string incomeType,
        string grossAmount,
        DateTime receivedDate,
        string? taxWithheld,
        bool? isTaxable
    )
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        var income = await _incomeRepository.FirstOrDefaultAsync(i =>
            i.Id == id && i.UserId == userId
        );
        if (income == null)
        {
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke income ini"
            );
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            SweetAlert.Error(TempData, "Input Gagal", "Source name harus diisi");
            return View(income);
        }

        if (string.IsNullOrWhiteSpace(incomeType))
        {
            SweetAlert.Error(TempData, "Input Gagal", "Income type harus diisi");
            return View(income);
        }

        var parsedGrossAmount = decimal.Parse(grossAmount ?? "0", CultureInfo.InvariantCulture);
        if (parsedGrossAmount <= 0)
        {
            SweetAlert.Error(TempData, "Input Gagal", "Gross amount harus lebih dari 0");
            return View(income);
        }

        var parsedTaxWithheld = !string.IsNullOrEmpty(taxWithheld)
            ? decimal.Parse(taxWithheld, CultureInfo.InvariantCulture)
            : 0;

        income.SourceName = sourceName;
        income.IncomeType = incomeType;
        income.GrossAmount = parsedGrossAmount;
        income.ReceivedDate = receivedDate;
        income.TaxWithheld = parsedTaxWithheld;
        // Checkbox submits true when checked, null when unchecked
        income.IsTaxable = isTaxable ?? false;

        // Preserve the original CreatedAt with UTC kind
        if (income.CreatedAt.Kind == DateTimeKind.Unspecified)
        {
            income.CreatedAt = DateTime.SpecifyKind(income.CreatedAt, DateTimeKind.Utc);
        }

        await _incomeRepository.UpdateAsync(income);

        _logger.LogInformation($"Income {id} berhasil diupdate oleh user {userId}");
        SweetAlert.Success(TempData, "Update Berhasil", "Income berhasil diupdate.");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = HttpContext.Session.GetString("UserId")!;

        var income = await _incomeRepository.FirstOrDefaultAsync(i =>
            i.Id == id && i.UserId == userId
        );
        if (income == null)
        {
            SweetAlert.Error(
                TempData,
                "Tidak Diizinkan",
                "Anda tidak memiliki akses ke income ini"
            );
            return RedirectToAction("Index");
        }

        try
        {
            await _incomeRepository.DeleteAsync(income);

            _logger.LogInformation($"Income {id} berhasil dihapus oleh user {userId}");
            SweetAlert.Success(TempData, "Hapus Berhasil", "Income berhasil dihapus.");
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning($"Concurrency error saat menghapus income {id} oleh user {userId}");

            var incomeStillExists = await _incomeRepository.AnyAsync(i => i.Id == id);
            if (!incomeStillExists)
            {
                SweetAlert.Success(
                    TempData,
                    "Sudah Dihapus",
                    "Income ini sudah dihapus sebelumnya."
                );
            }
            else
            {
                SweetAlert.Error(
                    TempData,
                    "Gagal",
                    "Income tidak dapat dihapus. Silakan coba lagi."
                );
            }
        }

        return RedirectToAction("Index");
    }
}
