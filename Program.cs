using System.Globalization;
using AccountingApp.Data;
using AccountingApp.Interfaces;
using AccountingApp.Middleware;
using AccountingApp.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES CONFIGURATION
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml"]);
});

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IUserExpenseRepository, UserExpenseRepository>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 2. MIDDLEWARE PIPELINE (Urutan Sangat Penting)

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseResponseCompression();
}

app.UseHttpsRedirection();

// Gunakan MapStaticAssets di .NET 9 untuk performa maksimal aset build-time
app.MapStaticAssets();

// Gunakan UseStaticFiles HANYA SEKALI dengan header cache yang benar
app.UseStaticFiles(
    new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache 1 tahun (31536000 detik) untuk mengatasi audit Lighthouse
            var cacheDuration = TimeSpan.FromDays(30).TotalSeconds;
            ctx.Context.Response.Headers.CacheControl = $"public,max-age={cacheDuration},immutable";
        },
    }
);

var supportedCultures = new[] { new CultureInfo("id-ID") };
app.UseRequestLocalization(
    new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("id-ID"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures,
    }
);

app.UseRouting();
app.UseSession();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();

// 3. ENDPOINT CONFIGURATION
app.MapControllerRoute(name: "default", pattern: "{controller=Authentication}/{action=Index}/{id?}")
    .WithStaticAssets(); // Mengintegrasikan routing dengan fitur aset .NET 9

app.Run();
