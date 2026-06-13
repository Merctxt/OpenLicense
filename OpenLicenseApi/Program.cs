using Scalar.AspNetCore;
using OpenLicenseApi.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

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
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
