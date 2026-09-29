using Microsoft.AspNetCore.Http;
using OpenLicense.Infrastructure.Auth;

namespace OpenLicense.Api.Middleware.Auth
{
    public class CookieToBearerMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieToBearerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.HeaderName))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}
