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
        _jwtSecret = TestEnvironment.JwtSecret;

        _dbPath = Path.Combine(AppContext.BaseDirectory, $"test_{Guid.NewGuid():N}.db");
        var connStr = $"Data Source={_dbPath};Pooling=false";

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestEnvironment.DatabaseConnection);
            builder.UseSetting("Jwt:SecretKey", _jwtSecret!);
            builder.UseSetting("Jwt:Issuer", TestEnvironment.JwtIssuer);
            builder.UseSetting("Jwt:Audience", TestEnvironment.JwtAudience);
            builder.UseSetting("RegistrationEnabled", "true");

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

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        _factory = null;
        _client = null;

        if (_dbPath != null)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Thread.Sleep(500);

            try { File.Delete(_dbPath); } catch { }
            try { File.Delete(_dbPath + ".wal"); } catch { }
            try { File.Delete(_dbPath + ".shm"); } catch { }
        }

        return Task.CompletedTask;
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
