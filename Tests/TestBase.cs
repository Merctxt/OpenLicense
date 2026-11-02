namespace OpenLicense.Tests;

public abstract class TestBase : IAsyncLifetime
{
    protected WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private string? _jwtSecret;
    private string? _dbPath;

    public HttpClient Client => _client ?? throw new InvalidOperationException("Factory not initialized.");

    public async Task InitializeAsync()
    {
        var config = LoadConfiguration();
        
        _jwtSecret = config["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("Jwt:SecretKey not found in appsettings.json");

        _dbPath = Path.Combine(AppContext.BaseDirectory, $"test_{Guid.NewGuid():N}.db");
        var connStr = $"Data Source={_dbPath}";

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Jwt:SecretKey", _jwtSecret!);
            builder.UseSetting("Jwt:Issuer", config["Jwt:Issuer"] ?? "OpenLicenseApi");
            builder.UseSetting("Jwt:Audience", config["Jwt:Audience"] ?? "OpenLicenseApiUsers");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlite(connStr);

                services.Replace(ServiceDescriptor.Scoped<AppDbContext>(sp => new AppDbContext(optionsBuilder.Options)));
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var initScope = _factory!.Services.CreateScope();
        var initDb = initScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await initDb.Database.EnsureCreatedAsync();
    }

    private static IConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        return builder.Build();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();

        if (_factory != null)
        {
            var dbPath = _dbPath;
            try { await _factory.DisposeAsync(); } catch { }
            _factory = null;
        }

        _client = null;
    }

    public async Task<User> RegisterUser(string? email = null, string? name = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var payload = new
        {
            Name = name ?? $"User {uniqueId}",
            Email = email ?? $"test.{uniqueId}@test.com",
            Password = password ?? "TestPass1!"
        };

        var response = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<User>();
        user.Should().NotBeNull();
        return user!;
    }

    public async Task<string> LoginAndGetToken(string email, string password)
    {
        var payload = new { Email = email, Password = password };
        var response = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/login", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        return result!.Token;
    }

    public async Task<(string token, User user)> GetAuthenticatedUser(string? email = null, string? name = null, string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var userEmail = email ?? $"test.{uniqueId}@test.com";
        var userName = name;
        var userPassword = password ?? "TestPass1!";
        
        var user = await RegisterUser(userEmail, userName, userPassword);
        var token = await LoginAndGetToken(user.Email, userPassword);
        return (token, user);
    }

    public string GenerateJwtToken(User user)
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
