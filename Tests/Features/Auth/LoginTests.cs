namespace OpenLicense.Tests.Features.Auth;

public class LoginTests : TestBase
{
    [Fact]
    public async Task ShouldLoginWithValidCredentials()
    {
        await RegisterUser(email: "login@test.com");
        var payload = new { Email = "login@test.com", Password = "TestPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldReturn401WithInvalidEmail()
    {
        await RegisterUser(email: "exists@test.com");
        var payload = new { Email = "nofind@test.com", Password = "TestPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturn401WithWrongPassword()
    {
        await RegisterUser(email: "pw@test.com");
        var payload = new { Email = "pw@test.com", Password = "WrongPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldNormalizeEmailInLogin()
    {
        await RegisterUser(email: "norm@test.com");
        var payload = new { Email = "NORM@TEST.COM", Password = "TestPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/login", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
