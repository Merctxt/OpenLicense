namespace OpenLicense.Tests.Features.Licenses;

public class ActivationToggleTests : TestBase
{
    [Fact]
    public async Task ShouldToggleActivationFromActiveToInactive()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activation = new Activation
        {
            LicenseId = licenseId,
            HardwareId = "HW-TOGGLE-001",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var togglePayload = new { LicenseId = licenseId, HardwareId = "HW-TOGGLE-001" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await toggleResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldToggleActivationFromInactiveToActive()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activation = new Activation
        {
            LicenseId = licenseId,
            HardwareId = "HW-TOGGLE-002",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = false
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var togglePayload = new { LicenseId = licenseId, HardwareId = "HW-TOGGLE-002" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await toggleResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseNotFound()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var togglePayload = new { LicenseId = Guid.NewGuid(), HardwareId = "HW-GHOST" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenActivationNotFound()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact4@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var togglePayload = new { LicenseId = licenseId, HardwareId = "HW-NOTFOUND" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseBelongsToAnotherUser()
    {
        var (licenseId1, token1) = await CreateLicenseAndGetId("toggleactowner@test.com");

        var (licenseId2, token2) = await CreateLicenseAndGetId("toggleactother@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activation = new Activation
        {
            LicenseId = licenseId1,
            HardwareId = "HW-TOGGLE-003",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var togglePayload = new { LicenseId = licenseId1, HardwareId = "HW-TOGGLE-003" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var togglePayload = new { LicenseId = Guid.NewGuid(), HardwareId = "HW-GHOST" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldPreventReactivatingForInactiveLicense()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact5@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var license = await dbContext.Licenses.FindAsync(licenseId);
        license!.Status = false;
        await dbContext.SaveChangesAsync();

        var activation = new Activation
        {
            LicenseId = licenseId,
            HardwareId = "HW-TOGGLE-004",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = false
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var togglePayload = new { LicenseId = licenseId, HardwareId = "HW-TOGGLE-004" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldAllowReactivateInactiveActivation()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("toggleact6@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activation = new Activation
        {
            LicenseId = licenseId,
            HardwareId = "HW-TOGGLE-005",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = false
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var togglePayload = new { LicenseId = licenseId, HardwareId = "HW-TOGGLE-005" };
        var toggleResponse = await Client.PutAsJsonAsync("/api/v1/licenses/activations/toggle", togglePayload);
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await toggleResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result!.IsActive.Should().BeTrue();
    }

    private async Task<(Guid licenseId, string token)> CreateLicenseAndGetId(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var productResponse = await Client.PostAsJsonAsync("/api/v1/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();

        var licenseResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = product!.Id, Name = "License", MaxActivations = 5 });
        var license = await licenseResponse.Content.ReadFromJsonAsync<License>();

        return (license!.Id, token);
    }

    public record ToggleResult(bool IsActive);
}
