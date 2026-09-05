namespace OpenLicense.Tests.Features.Auth;

public class ApiKeyTests : TestBase
{
    [Fact]
    public async Task ShouldCreateApiKeyWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("apikey@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Name = "My API Key" };
        var response = await Client.PostAsJsonAsync("/api/auth/apikey", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiKey = await response.Content.ReadFromJsonAsync<CreateApiKeyResponse>();
        apiKey.Should().NotBeNull();
        apiKey!.Id.Should().NotBeEmpty();
        apiKey.Name.Should().Be("My API Key");
        apiKey.ApiKey.Should().StartWith("api_");
        apiKey.ApiKey.Length.Should().BeGreaterThan(64);
    }

    [Fact]
    public async Task ShouldRespectLimitOf3Keys()
    {
        var (token, user) = await GetAuthenticatedUser("limit@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (int i = 0; i < 3; i++)
        {
            var payload = new { Name = $"Key {i}" };
            var response = await Client.PostAsJsonAsync("/api/auth/apikey", payload);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var fourthPayload = new { Name = "Fourth Key" };
        var fourthResponse = await Client.PostAsJsonAsync("/api/auth/apikey", fourthPayload);

        fourthResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldDeleteApiKey()
    {
        var (token, user) = await GetAuthenticatedUser("delapikey@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createPayload = new { Name = "Delete Me" };
        var createResponse = await Client.PostAsJsonAsync("/api/auth/apikey", createPayload);
        var apiKey = await createResponse.Content.ReadFromJsonAsync<CreateApiKeyResponse>();

        var deletePayload = new { ApiKeyId = apiKey!.Id };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/auth/apikey", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var meResponse = await Client.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await meResponse.Content.ReadFromJsonAsync<GetUserResponse>();
        me!.ApiKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturn404WhenDeletingNotFoundApiKey()
    {
        var (token, user) = await GetAuthenticatedUser("notfound@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deletePayload = new { ApiKeyId = Guid.NewGuid() };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/auth/apikey", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public record CreateApiKeyResponse(Guid Id, string Name, string ApiKey, DateTime CreatedAt, bool IsActive);
    public record GetUserResponse(Guid Id, string Name, string Email, DateTime CreatedAt, bool IsSuspended, int ProductLimit, int LicenseLimit, List<ApiKeyItem> ApiKeys);
    public record ApiKeyItem(Guid Id, string Name, string ApiKey, DateTime CreatedAt, bool IsActive);
}
