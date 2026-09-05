using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Data;

namespace OpenLicenseApi.Middleware
{
    public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "X-Api-Key";

        private readonly AppDbContext _dbContext;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            AppDbContext dbContext)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderName, out var rawHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }

            var rawApiKey = rawHeaderValues.ToString();
            if (string.IsNullOrWhiteSpace(rawApiKey))
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            var keyHash = ComputeSha256Hex(rawApiKey.Trim());

            var apiKey = await _dbContext.ApiKeys
                .FirstOrDefaultAsync(k => k.KeyHash == keyHash);

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            if (!apiKey.IsActive)
            {
                return AuthenticateResult.Fail("Inactive API key.");
            }

            apiKey.LastUsedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiKey.UserId.ToString()),
                new Claim("api_key_id", apiKey.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return AuthenticateResult.Success(ticket);
        }

        private static string ComputeSha256Hex(string input)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }
    }
}
