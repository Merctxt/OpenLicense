using Scalar.AspNetCore;
using OpenLicenseApi.Data;
using Microsoft.EntityFrameworkCore;
using OpenLicenseApi.Services;
using DotNetEnv;
using OpenLicenseApi.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Text;

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
            /* - Apenas os Endpoints de Licença devem aparecer na documentação pública da API,
            pois são usados para a integração direta com os clientes. 
            Os Endpoints de Produto e Auth são usados apenas internamente pelo dashboard, 
            então não precisam aparecer na documentação pública.
            builder.Services.AddOpenApi("public", options =>
            {
                options.ShouldInclude = description =>
                    description.GroupName == "public";
            }); */

            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Use: Bearer {seu_token_jwt}"
                    };

                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, context, _) =>
                {
                    var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<IAuthorizeData>()
                        .Any();

                    if (!hasAuthorize)
                    {
                        return Task.CompletedTask;
                    }

                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }] = Array.Empty<string>()
                    });

                    return Task.CompletedTask;
                });
            });

            var jwtSecret = builder.Configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey was not found.");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
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
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            #endregion
            
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            #region Middleware Setup

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
