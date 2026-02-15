using OpenExpenseApp.Enums;

namespace OpenExpenseApp.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        // Tambahkan system paths agar tidak kena redirect
        private static readonly string[] PublicPaths =
        [
            "/authentication",
            "/css",
            "/js",
            "/lib",
            "/_framework", // .NET internal
            "/_vs", // Visual Studio internal
            "/.well-known", // Browser dev tools
        ];

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

            // 1. Bypass untuk Root, Public Paths, dan file statis lainnya
            if (path == "/" || IsPublicPath(path) || IsStaticFile(path))
            {
                await _next(context);
                return;
            }

            // 2. Check Session
            var userId = context.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning($"Session expired or unauthorized access to: {path}");

                // Gunakan Session untuk menyimpan pesan alert sementara
                context.Session.SetString("AlertType", AlertType.Warning.ToString());
                context.Session.SetString("AlertTitle", "Session Expired");
                context.Session.SetString("AlertMessage", "Silakan login kembali");
                await context.Session.CommitAsync();

                // Redirect ke landing page (Authentication/Index)
                context.Response.Redirect("/");
                return;
            }

            await _next(context);
        }

        private bool IsPublicPath(string path)
        {
            return PublicPaths.Any(p => path.StartsWith(p));
        }

        private bool IsStaticFile(string path)
        {
            // Menangani MapStaticAssets di .NET 9 yang seringkali
            // memiliki fingerprint unik di URL
            return path.Contains(".css")
                || path.Contains(".js")
                || path.Contains(".png")
                || path.Contains(".woff")
                || path.Contains("browserrefresh");
        }
    }
}
