namespace OpenLicense.Tests;

public abstract class TestBase : IAsyncLifetime
{
    protected WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private string? _jwtSecret;

    public HttpClient Client => _client ?? throw new InvalidOperationException("Factory not initialized.");

    public async Task InitializeAsync()
    {
        var config = LoadConfiguration();
        
        _jwtSecret = config["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("Jwt:SecretKey not found in appsettings.json");

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.json");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("database_connection", connectionString);
            builder.UseSetting("Jwt:SecretKey", _jwtSecret!);
            builder.UseSetting("Jwt:Issuer", config["Jwt:Issuer"] ?? "OpenLicenseApi");
            builder.UseSetting("Jwt:Audience", config["Jwt:Audience"] ?? "OpenLicenseApiUsers");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString, npgsql =>
                        npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await CleanDatabase();
    }

    private static IConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        return builder.Build();
    }

    private async Task CleanDatabase()
    {
        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var tables = new[] { "Activation", "ApiKey", "License", "Products", "Users" };
        foreach (var table in tables)
        {
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{table}\" CASCADE;");
            }
            catch { }
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    public async Task<Users> RegisterUser(string? email = null, string? name = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var payload = new
        {
            Name = name ?? $"User {uniqueId}",
            Email = email ?? $"test.{uniqueId}@test.com",
            Password = password ?? "TestPass1!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<Users>();
        user.Should().NotBeNull();
        return user!;
    }

    public async Task<string> LoginAndGetToken(string email, string password)
    {
        var payload = new { Email = email, Password = password };
        var response = await Client.PostAsJsonAsync("/api/auth/login", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        return result!.Token;
    }

    public async Task<(string token, Users user)> GetAuthenticatedUser(string? email = null, string? name = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var userEmail = email ?? $"test.{uniqueId}@test.com";
        var userName = name;
        var userPassword = password ?? "TestPass1!";
        
        var user = await RegisterUser(userEmail, userName, userPassword);
        var token = await LoginAndGetToken(user.Email, userPassword);
        return (token, user);
    }

    public string GenerateJwtToken(Users user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "OpenLicenseApi",
            audience: "OpenLicenseApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginResponse(string Token);
}
