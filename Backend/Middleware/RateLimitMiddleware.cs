using Microsoft.AspNetCore.Http;
using OpenLicenseApi.Services;

namespace OpenLicenseApi.Middleware
{
    /// <summary>
    /// Middleware that applies rate limiting to /api/auth/login and /api/auth/register.
    /// Uses IP-based sliding windows — no external dependencies.
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimiterService _rateLimiter;

        // Rate limits: per endpoint path prefix -> (maxRequests, window)
        private static readonly Dictionary<string, (int Max, TimeSpan Window)> _limits = new()
        {
            { "/api/auth/login", (10, TimeSpan.FromMinutes(1)) },          // 10 attempts/min
            { "/api/auth/register", (5, TimeSpan.FromMinutes(5)) },       // 5 requests/5min
            { "/api/auth/forgot-password", (3, TimeSpan.FromMinutes(5)) }, // 3 requests/5min
            { "/api/auth/reset-password/verify", (6, TimeSpan.FromMinutes(5)) }, // 6 requests/5min
            { "/api/auth/reset-password", (3, TimeSpan.FromMinutes(5)) }   // 3 requests/5min
        };

        public RateLimitMiddleware(RequestDelegate next, IRateLimiterService rateLimiter)
        {
            _next = next;
            _rateLimiter = rateLimiter;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Only apply to POST requests on auth endpoints
            if (method != "POST" || (_limits.Keys.FirstOrDefault(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)) is not string matchedPrefix))
            {
                await _next(context);
                return;
            }

            var config = _limits[matchedPrefix];

            // Build rate limit key from IP address
            var ip = context.Connection.RemoteIpAddress?.ToString()
                    ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? "unknown";

            var key = $"{ip}:{matchedPrefix}";

            if (!_rateLimiter.IsAllowed(key, config.Max, config.Window))
            {
                context.Response.StatusCode = 429;
                context.Response.Headers.RetryAfter = ((int)config.Window.TotalSeconds).ToString();
                await context.Response.WriteAsJsonAsync(new { message = "Too many requests. Please try again later." });
                return;
            }

            await _next(context);
        }
    }
}
