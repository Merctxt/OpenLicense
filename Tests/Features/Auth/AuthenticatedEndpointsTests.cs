namespace OpenLicense.Tests.Features.Auth;

public class AuthenticatedEndpointsTests : TestBase
{
    [Fact]
    public async Task ShouldReturnUserWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("me@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<Users>();
        me.Should().NotBeNull();
        me!.Id.Should().Be(user.Id);
        me.Name.Should().Be(user.Name);
        me.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldUpdateNameWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("update@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Name = "Updated Name" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Users>();
        updated!.Name.Should().Be("Updated Name");
        updated.Email.Should().Be("update@test.com");
    }

    [Fact]
    public async Task ShouldUpdateEmailWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("old@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Email = "new@test.com" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Users>();
        updated!.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task ShouldUpdatePasswordWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("pwup@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Password = "NewPass1!" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newLoginPayload = new { Email = "pwup@test.com", Password = "NewPass1!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", newLoginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldReturn400WhenEmailAlreadyTaken()
    {
        var (token1, user1) = await GetAuthenticatedUser("user1@test.com");
        await RegisterUser(email: "user2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var payload = new { Email = "user2@test.com" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturn401WhenUpdatingWithoutAuth()
    {
        var payload = new { Name = "Hacker" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldDeleteAccountWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("del@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync("/api/auth");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var loginPayload = new { Email = "del@test.com", Password = "TestPass1!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturn401WhenDeletingWithoutAuth()
    {
        var response = await Client.DeleteAsync("/api/auth");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldLogout()
    {
        var (token, user) = await GetAuthenticatedUser("logout@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/auth/logout", new { });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = new { Email = "logout@test.com", Password = "TestPass1!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
