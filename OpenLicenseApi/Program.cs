using Scalar.AspNetCore;
using OpenLicenseApi.Data;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Services;
using OpenLicenseApi.Middleware;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;

namespace OpenLicenseApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // ── Controllers ──────────────────────────────────────────────
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // ── Database ─────────────────────────────────────────────────
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? builder.Configuration["database_connection"]
                ?? Environment.GetEnvironmentVariable("database_connection")
                ?? throw new InvalidOperationException("Database connection string was not found.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null))
            );

            // ── OpenAPI / Scalar ─────────────────────────────────────────
            builder.Services.AddOpenApiConfiguration();

            // ── Authentication (JWT + API Key) ───────────────────────────
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddAuthorization();

            // ── Business Services ────────────────────────────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ILicenseService, LicensesService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddSingleton<IRateLimiterService, RateLimiterService>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.AddScoped<IEmailService, EmailService>();

            var app = builder.Build();

            // ── Forwarded Headers (for reverse proxy / Docker) ───────────
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            // ── Health & Docs ────────────────────────────────────────────
            app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).ExcludeFromDescription();
            app.MapOpenApi();
            app.MapScalarApiReference();

            // ── CORS ─────────────────────────────────────────────────────
            var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
            app.UseCors(options =>
                options.WithOrigins(frontendUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );

            // ── Middleware Pipeline ──────────────────────────────────────
            app.UseMiddleware<RateLimitMiddleware>();
            app.UseMiddleware<CookieToBearerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
