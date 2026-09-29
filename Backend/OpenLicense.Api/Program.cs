using Scalar.AspNetCore;
using OpenLicense.Api.Middleware.Documentation;
using OpenLicense.Api.Middleware.Observability;
using OpenLicense.Api.Middleware.Security;
using OpenLicense.Api.Middleware.ErrorHandling;
using OpenLicense.Api.Middleware.Auth;
using OpenLicense.Infrastructure;
using OpenLicense.Infrastructure.Data;
using OpenLicense.Infrastructure.Services;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Versioning;
using MediatR;

namespace OpenLicense.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // ── Telemetry (Traces + Metrics + Logs → OpenObserve) ────────
            builder.AddOpenTelemetry("OpenLicenseApi");

            // ── Controllers ──────────────────────────────────────────────
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // ── Database (from Infrastructure) ───────────────────────────
            builder.Services.AddInfrastructure(builder.Configuration);

            // ── MediatR ──────────────────────────────────────────────────
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(OpenLicense.Application.Commands.Auth.RegisterUserCommand).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // ── OpenAPI / Scalar ─────────────────────────────────────────
            builder.Services.AddOpenApiConfiguration();

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
            app.MapScalarApiReference(options =>
            {
                options.Title = "OpenLicense API";
                options.Theme = ScalarTheme.Alternate;
            });

            // ── CORS ─────────────────────────────────────────────────────
            var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
            app.UseCors(options =>
                options.WithOrigins(frontendUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );

            // ── Middleware Pipeline ──────────────────────────────────────
            app.UseMiddleware<AuditMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RateLimitMiddleware>();
            app.UseMiddleware<CookieToBearerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
