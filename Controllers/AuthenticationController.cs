using System.Diagnostics;
using AccountingApp.Data;
using AccountingApp.Helpers;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using AccountingApp.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

namespace AccountingApp.Controllers;

public class AuthenticationController : Controller
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IUserRepository _userRepository;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        IUserRepository userRepository
    )
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new LoginViewModel { Username = "", Password = "" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SweetAlertHelper.Error(TempData, "Login Gagal", "Username dan password harus diisi");
            return View(model); // Pass the model back to preserve input
        }

        var user = await _userRepository.GetUserByUsernameAsync(model.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            SweetAlertHelper.Error(
                TempData,
                "Login Gagal",
                "Gagal melakukan login. Periksa kembali username dan password Anda."
            );
            return RedirectToAction("Index");
        }

        HttpContext.Session.SetString("UserId", user.Id);
        HttpContext.Session.SetString("Username", user.Username);

        _logger.LogInformation($"User {model.Username} berhasil login");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Registration(
        string fullName,
        string username,
        string password,
        string email
    )
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            SweetAlertHelper.Error(
                TempData,
                "Registrasi Gagal",
                "Username dan password harus diisi"
            );
            return View("Registration");
        }

        // Check if username already exists
        if (await _userRepository.IsUserExistsAsync(username))
        {
            SweetAlertHelper.Error(
                TempData,
                "Registrasi Gagal",
                "Tidak dapat melakukan registrasi."
            );
            return RedirectToAction("Registration");
        }

        // Create new user
        var newUser = new User
        {
            Id = Ulid.NewUlid().ToString(),
            FullName = fullName,
            Username = username,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };

        await _userRepository.CreateAsync(newUser);

        _logger.LogInformation($"User {username} berhasil registrasi");
        SweetAlertHelper.Success(
            TempData,
            "Registrasi Berhasil",
            "Registrasi berhasil. Silakan login."
        );
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
