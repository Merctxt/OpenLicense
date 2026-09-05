namespace OpenLicense.Tests.Features.Licenses;

public class GetLicensesTests : TestBase
{
    [Fact]
    public async Task ShouldReturnEmptyListWhenNoLicenses()
    {
        var (productId, token) = await CreateProductAndGetToken("getlic1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/licenses?productId={productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var licenses = await response.Content.ReadFromJsonAsync<List<License>>();
        licenses.Should().NotBeNull();
        licenses.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnLicensesForProduct()
    {
        var (productId, token) = await CreateProductAndGetToken("getlic2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateLicense(token, productId, "License 1");

        var response = await Client.GetAsync($"/api/licenses?productId={productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var licenses = await response.Content.ReadFromJsonAsync<List<License>>();
        licenses.Should().NotBeNull();
        licenses!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ShouldReturn400WhenProductIdMissing()
    {
        var (productId, token) = await CreateProductAndGetToken("getlic3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/licenses");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturn404WhenProductNotFound()
    {
        var (productId, token) = await CreateProductAndGetToken("getlic4@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/licenses?productId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new Users
        {
            Id = Guid.NewGuid(),
            Name = "Auth Test User",
            Email = $"auth-test-{Guid.NewGuid():N}@test.com",
            PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<Users>().HashPassword(null!, "TestPass1!"),
            ProductLimit = 5,
            LicenseLimit = 10,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Test Product",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/licenses?productId={product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturn404WhenProductBelongsToAnotherUser()
    {
        var (productId1, token1) = await CreateProductAndGetToken("owner@test.com");
        
        var (productId2, token2) = await CreateProductAndGetToken("other@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var response = await Client.GetAsync($"/api/licenses?productId={productId1}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<(Guid productId, string token)> CreateProductAndGetToken(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        return (product!.Id, token);
    }

    private async Task CreateLicense(string token, Guid productId, string name)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = new { ProductId = productId, Name = name, MaxActivations = 3 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class CreateLicenseTests : TestBase
{
    [Fact]
    public async Task ShouldCreateLicenseWithValidData()
    {
        var (productId, token) = await CreateProductAndGetToken("crelic1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = productId, Name = "License 1", MaxActivations = 3 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var license = await response.Content.ReadFromJsonAsync<License>();
        license.Should().NotBeNull();
        license!.ProductId.Should().Be(productId);
        license.Name.Should().Be("License 1");
        license.MaxActivations.Should().Be(3);
        license.LicenseKey.Should().NotBeNullOrEmpty();
        license.Status.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldGenerateUniqueLicenseKey()
    {
        var (productId, token) = await CreateProductAndGetToken("crelic2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateLicenseRequest(token, productId, "License A");
        await CreateLicenseRequest(token, productId, "License B");

        var allResponse = await Client.GetAsync($"/api/licenses?productId={productId}");
        var licenses = await allResponse.Content.ReadFromJsonAsync<List<License>>();

        var keys = licenses!.Select(l => l.LicenseKey).ToList();
        keys.Distinct().Count().Should().Be(keys.Count);
    }

    [Fact]
    public async Task ShouldReturn400WhenProductIdIsEmpty()
    {
        var (productId, token) = await CreateProductAndGetToken("crelic3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = Guid.Empty, Name = "Bad", MaxActivations = 1 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturn400WhenMaxActivationsZero()
    {
        var (productId, token) = await CreateProductAndGetToken("crelic4@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = productId, Name = "Bad", MaxActivations = 0 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturn404WhenProductNotFound()
    {
        var (productId, token) = await CreateProductAndGetToken("crelic5@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = Guid.NewGuid(), Name = "Ghost", MaxActivations = 1 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var payload = new { ProductId = Guid.NewGuid(), Name = "Hacker", MaxActivations = 1 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid productId, string token)> CreateProductAndGetToken(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        return (product!.Id, token);
    }

    private async Task CreateLicenseRequest(string token, Guid productId, string name)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = new { ProductId = productId, Name = name, MaxActivations = 1 };
        var response = await Client.PostAsJsonAsync("/api/licenses", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class UpdateLicenseTests : TestBase
{
    [Fact]
    public async Task ShouldUpdateName()
    {
        var (productId, token) = await CreateProductAndGetToken("uplic1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId, Name = "Old", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var updatePayload = new { LicenseId = license!.Id, Name = "Updated" };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<License>();
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task ShouldUpdateMaxActivations()
    {
        var (productId, token) = await CreateProductAndGetToken("uplic2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId, Name = "Test", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var updatePayload = new { LicenseId = license!.Id, MaxActivations = 10 };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<License>();
        updated!.MaxActivations.Should().Be(10);
    }

    [Fact]
    public async Task ShouldUpdateStatus()
    {
        var (productId, token) = await CreateProductAndGetToken("uplic3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId, Name = "Test", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var updatePayload = new { LicenseId = license!.Id, Status = false };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<License>();
        updated!.Status.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseNotFound()
    {
        var (productId, token) = await CreateProductAndGetToken("uplic4@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updatePayload = new { LicenseId = Guid.NewGuid(), Name = "Ghost" };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseBelongsToAnotherUser()
    {
        var (productId1, token1) = await CreateProductAndGetToken("ownerlic@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId1, Name = "Owner License", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var (productId2, token2) = await CreateProductAndGetToken("otherlic@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var updatePayload = new { LicenseId = license!.Id, Name = "Hacked" };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn400WhenLicenseAlreadyInThatStatus()
    {
        var (productId, token) = await CreateProductAndGetToken("uplic5@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId, Name = "Test", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var updatePayload = new { LicenseId = license!.Id, Status = true };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var updatePayload = new { LicenseId = Guid.NewGuid(), Name = "Hacked" };
        var updateResponse = await Client.PutAsJsonAsync("/api/licenses", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid productId, string token)> CreateProductAndGetToken(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        return (product!.Id, token);
    }
}

public class DeleteLicenseTests : TestBase
{
    [Fact]
    public async Task ShouldDeleteLicenseWhenOwned()
    {
        var (productId, token) = await CreateProductAndGetToken("delic1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId, Name = "Delete Me", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var deletePayload = new { LicenseId = license!.Id };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/licenses", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var allResponse = await Client.GetAsync($"/api/licenses?productId={productId}");
        var licenses = await allResponse.Content.ReadFromJsonAsync<List<License>>();
        licenses!.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseNotFound()
    {
        var (productId, token) = await CreateProductAndGetToken("delic2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deletePayload = new { LicenseId = Guid.NewGuid() };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/licenses", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseBelongsToAnotherUser()
    {
        var (productId1, token1) = await CreateProductAndGetToken("ownerdel@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = productId1, Name = "Owner License", MaxActivations = 1 });
        var license = await createResponse.Content.ReadFromJsonAsync<License>();

        var (productId2, token2) = await CreateProductAndGetToken("otherdel@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var deletePayload = new { LicenseId = license!.Id };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/licenses", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var deletePayload = new { LicenseId = Guid.NewGuid() };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/licenses", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid productId, string token)> CreateProductAndGetToken(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        return (product!.Id, token);
    }
}

public class ActivationsTests : TestBase
{
    [Fact]
    public async Task ShouldReturnEmptyWhenNoActivations()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("act1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/licenses/activations?licenseId={licenseId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activations = await response.Content.ReadFromJsonAsync<List<Activation>>();
        activations.Should().NotBeNull();
        activations.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnActivationsWhenExist()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("act2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activation = new Activation
        {
            LicenseId = licenseId,
            HardwareId = "HW-TEST-001",
            ActivatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true
        };
        dbContext.Activations.Add(activation);
        await dbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/licenses/activations?licenseId={licenseId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activations = await response.Content.ReadFromJsonAsync<List<Activation>>();
        activations.Should().NotBeNull();
        activations!.Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldReturn404WhenLicenseNotFound()
    {
        var (licenseId, token) = await CreateLicenseAndGetId("act3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/licenses/activations?licenseId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/api/licenses/activations?licenseId=" + Guid.NewGuid());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<(Guid licenseId, string token)> CreateLicenseAndGetId(string email)
    {
        var (token, user) = await GetAuthenticatedUser(email);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var productResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = $"Product-{email.Split('@')[0]}" });
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();

        var licenseResponse = await Client.PostAsJsonAsync("/api/licenses", new { ProductId = product!.Id, Name = "License", MaxActivations = 1 });
        var license = await licenseResponse.Content.ReadFromJsonAsync<License>();

        return (license!.Id, token);
    }
}
