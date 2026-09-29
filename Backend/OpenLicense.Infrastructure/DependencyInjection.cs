using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenLicense.Application.Services.Interfaces;
using OpenLicense.Infrastructure.Auth;
using OpenLicense.Infrastructure.Data;
using OpenLicense.Infrastructure.Services;
using System.Text;

namespace OpenLicense.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["database_connection"]
            ?? Environment.GetEnvironmentVariable("database_connection")
            ?? throw new InvalidOperationException("Database connection string was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null))
        );

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRateLimiterService, RateLimiterService>();
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, EmailService>();

        services.Configure<ReportSettings>(configuration.GetSection("ReportSettings"));
        services.AddScoped<IReportService, ReportService>();
        services.AddHostedService<ReportEmailBackgroundService>();

        // Auth
        services.AddAuthenticationServices(configuration);
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey was not found.");
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "SmartAuth";
            options.DefaultAuthenticateScheme = "SmartAuth";
            options.DefaultChallengeScheme = "SmartAuth";
        })
        .AddPolicyScheme("SmartAuth", "JWT or API Key", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var authorization = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrWhiteSpace(authorization)
                    && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                if (context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.HeaderName))
                {
                    return ApiKeyAuthenticationHandler.SchemeName;
                }

                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
                ValidIssuer = jwtIssuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName,
            _ => { }
        );

        return services;
    }
}
