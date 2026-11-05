namespace OpenLicense.Tests.Features.Auth;

public class ApiKeyToggleTests : TestBase
{
    [Fact]
    public async Task ShouldToggleApiKeyFromActiveToInactive()
    {
        var (token, user) = await GetAuthenticatedUser("toggle1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/apikey", new { Name = "Toggle Me" });
        var apiKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        apiKey.Should().NotBeNull();
        apiKey!.IsActive.Should().BeTrue();

        var togglePayload = new { ApiKeyId = apiKey.Id };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await toggleResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();

        var meResponse = await Client.GetAsync($"{ApiVersion.Path}/auth/me");
        var me = await meResponse.Content.ReadFromJsonAsync<GetUserResponse>();
        me!.ApiKeys.Should().NotBeEmpty();
        me.ApiKeys.First().IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldToggleApiKeyFromInactiveToActive()
    {
        var (token, user) = await GetAuthenticatedUser("toggle2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/apikey", new { Name = "Toggle Me 2" });
        var apiKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        apiKey.Should().NotBeNull();

        var togglePayload = new { ApiKeyId = apiKey.Id };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var toggleBackPayload = new { ApiKeyId = apiKey.Id };
        var toggleBackResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", toggleBackPayload);
        toggleBackResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await toggleBackResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturn404WhenTogglingNotFoundApiKey()
    {
        var (token, user) = await GetAuthenticatedUser("toggle3@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var togglePayload = new { ApiKeyId = Guid.NewGuid() };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenTogglingApiKeyBelongsToAnotherUser()
    {
        var (token1, user1) = await GetAuthenticatedUser("toggleowner@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/apikey", new { Name = "Owner Key" });
        var apiKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        var (token2, user2) = await GetAuthenticatedUser("toggleother@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var togglePayload = new { ApiKeyId = apiKey.Id };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var togglePayload = new { ApiKeyId = Guid.NewGuid() };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);

        toggleResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldNotAllowApiKeyToWorkWhenInactive()
    {
        var (token, user) = await GetAuthenticatedUser("toggle4@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/auth/apikey", new { Name = "Disable Me" });
        var apiKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        apiKey.Should().NotBeNull();

        var togglePayload = new { ApiKeyId = apiKey.Id };
        var toggleResponse = await Client.PutAsJsonAsync($"{ApiVersion.Path}/auth/apikey/toggle", togglePayload);
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await toggleResponse.Content.ReadFromJsonAsync<ToggleResult>();
        result!.IsActive.Should().BeFalse();

        var validateResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/licenses/validate", new
        {
            licenseKey = "A1B2-C3D4-E5F6-G7H8",
            hardwareId = "HW-TEST"
        });
        validateResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public record CreateApiKeyResponse(Guid Id, string Name, string ApiKey, DateTime CreatedAt, bool IsActive);
    public record GetUserResponse(Guid Id, string Name, string Email, DateTime CreatedAt, bool IsSuspended, int ProductLimit, int LicenseLimit, List<ApiKeyItem> ApiKeys);
    public record ApiKeyItem(Guid Id, string Name, string ApiKey, DateTime CreatedAt, bool IsActive);
    public record ToggleResult(bool IsActive);
}
