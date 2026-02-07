using System.Linq;
using System.Threading.Tasks;
using AccountingApp.Enums;
using AccountingApp.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AccountingApp.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        // Routes yang tidak perlu session check (public routes)
        private static readonly string[] PublicRoutes = new[]
        {
            "/",
            "/authentication",
            "/css",
            "/js",
            "/lib",
        };

        public SessionValidationMiddleware(
            RequestDelegate next,
            ILogger<SessionValidationMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Check if route membutuhkan session
            bool isPublicRoute = IsPublicRoute(path);

            if (!isPublicRoute)
            {
                var userId = context.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning($"Session expired for path: {path}");

                    // Set alert dengan TempData style (using Session)
                    context.Session.SetString("AlertType", AlertType.Warning.ToString());
                    context.Session.SetString("AlertTitle", "Session Expired");
                    context.Session.SetString("AlertMessage", "Silakan login terlebih dahulu");
                    await context.Session.CommitAsync();

                    // Redirect ke login page
                    context.Response.StatusCode = StatusCodes.Status302Found;
                    context.Response.Headers.Location = "/";
                    return;
                }
            }

            await _next(context);
        }

        private bool IsPublicRoute(string path)
        {
            // Exact match untuk root
            if (path == "/" || path == "")
                return true;

            // Starts with untuk controller routes dan assets
            return PublicRoutes.Skip(1).Any(route => path.StartsWith(route));
        }
    }
}
