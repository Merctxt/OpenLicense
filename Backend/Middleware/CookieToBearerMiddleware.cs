using Microsoft.AspNetCore.Http;

namespace OpenLicenseApi.Middleware
{
    /// <summary>
    /// Reads the JWT from an HttpOnly cookie and injects it into the Authorization header.
    /// This allows the existing JWT Bearer authentication pipeline to work unchanged.
    /// </summary>
    public class CookieToBearerMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieToBearerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}
