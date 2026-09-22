using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace OpenLicenseApi.Middleware.Observability
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        private static readonly Dictionary<string, string> _auditEvents = new(StringComparer.OrdinalIgnoreCase)
        {
            { "/api/auth/register", "user_register" },
            { "/api/auth/login", "user_login" },
            { "/api/auth/logout", "user_logout" },
            { "/api/auth/apikey", "api_key_operation" },
            { "/api/auth/forgot-password", "password_reset_requested" },
            { "/api/auth/reset-password", "password_reset" },
            { "/api/licenses/validate", "license_validate" },
            { "/api/licenses/deactivate", "license_deactivate" },
            { "/api/licenses/deactivate-by-jwt", "license_deactivate_jwt" },
        };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            var clientIp = ResolveClientIp(context);
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;

            context.Items["ClientIp"] = clientIp;

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();

                var statusCode = context.Response.StatusCode;
                var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var correlationId = context.TraceIdentifier;

                var logLevel = statusCode switch
                {
                    >= 500 => LogLevel.Error,
                    >= 400 => LogLevel.Warning,
                    _ => LogLevel.Information
                };

                _logger.Log(
                    logLevel,
                    0,
                    new
                    {
                        Timestamp = DateTime.UtcNow.ToString("O"),
                        Method = method,
                        Path = path,
                        StatusCode = statusCode,
                        ClientIp = clientIp,
                        DurationMs = sw.ElapsedMilliseconds,
                        UserAgent = userAgent,
                        CorrelationId = correlationId,
                        User = userId ?? "anonymous"
                    },
                    null,
                    (state, ex) => $"[{state.Timestamp}] {state.Method} {state.Path} -> {state.StatusCode} | IP={state.ClientIp} | Duration={state.DurationMs}ms | UserAgent={state.UserAgent} | CorrelationId={state.CorrelationId} | User={state.User}"
                );

                var auditEvent = GetAuditEvent(path, method);
                if (auditEvent != null && (statusCode >= 200 && statusCode < 400))
                {
                    var outcome = statusCode == 200 || statusCode == 201 || statusCode == 204 ? "success" : "failed";
                    _logger.LogInformation(
                        "[AUDIT] {EventType} | Ip={ClientIp} | User={UserId} | Endpoint={Path} | Method={Method} | Outcome={Outcome} | StatusCode={StatusCode} | CorrelationId={CorrelationId}",
                        auditEvent,
                        clientIp,
                        userId ?? "anonymous",
                        path,
                        method,
                        outcome,
                        statusCode,
                        correlationId
                    );
                }
            }
        }

        private static string ResolveClientIp(HttpContext context)
        {
            var cfIp = context.Request.Headers["cf-connecting-ip"].FirstOrDefault();
            if (IsValidPublicIp(cfIp!))
                return cfIp!;

            var realIp = context.Request.Headers["x-real-ip"].FirstOrDefault();
            if (IsValidPublicIp(realIp!))
                return realIp!;

            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                foreach (var ip in forwardedFor.Split(','))
                {
                    var clean = ip.Trim().Trim('[').Trim(']');
                    if (IsValidPublicIp(clean))
                        return clean;
                }
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (IsValidPublicIp(remoteIp!))
                return remoteIp!;

            return remoteIp ?? "unknown";
        }

        private static bool IsValidPublicIp(string? ip)
        {
            if (string.IsNullOrEmpty(ip))
                return false;

            if (!System.Net.IPAddress.TryParse(ip, out var address))
                return false;

            return !IsPrivateOrReserved(address);
        }

        private static bool IsPrivateOrReserved(System.Net.IPAddress address)
        {
            if (IPAddress.IsLoopback(address)) return true;

            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal;

            var bytes = address.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168);
        }

        private static string? GetAuditEvent(string path, string method)
        {
            var matched = _auditEvents.Keys.FirstOrDefault(k => path.StartsWith(k, StringComparison.OrdinalIgnoreCase));
            if (matched != null)
            {
                var eventType = _auditEvents[matched];
                if (method == "GET" && eventType.Contains("operation"))
                {
                    return eventType.Replace("_operation", "_list");
                }
                return eventType;
            }
            return null;
        }
    }
}
