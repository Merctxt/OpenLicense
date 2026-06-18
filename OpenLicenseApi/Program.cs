using Scalar.AspNetCore;
using OpenLicenseApi.Data;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Services;
using DotNetEnv;
using OpenLicenseApi.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Text;
using OpenLicenseApi.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
namespace OpenLicenseApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region DB Setup
                
            // Ensure .env values are available for local CLI/runtime scenarios.
            Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.   

            builder.Services.AddControllers();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = builder.Configuration["database_connection"]
                    ?? Environment.GetEnvironmentVariable("database_connection");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString
                ?? throw new InvalidOperationException("Database connection string was not found. Set ConnectionStrings:DefaultConnection or database_connection (including .env).")
                ));
       
            #endregion

            #region OpenAPI Setup

            builder.Services.AddOpenApiConfiguration();

            var jwtSecret = builder.Configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey was not found.");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthenticationServices(builder.Configuration);

            builder.Services.AddAuthorization();

            #endregion
            
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ILicenseService, LicensesService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapOpenApi();
            app.MapScalarApiReference();

            #region Middleware Setup

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(options =>
            {
                options.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
