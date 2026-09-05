namespace OpenLicense.Tests.Features.Auth;

public class RegisterTests : TestBase
{
    [Fact]
    public async Task ShouldRegisterWithValidData()
    {
        var payload = new { Name = "Valid User", Email = "valid@test.com", Password = "ValidPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<Users>();
        user.Should().NotBeNull();
        user!.Name.Should().Be("Valid User");
        user.Email.Should().Be("valid@test.com");
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturn400WhenEmailAlreadyExists()
    {
        await RegisterUser(email: "dup@test.com");
        var payload = new { Name = "User 2", Email = "dup@test.com", Password = "TestPass1!" };

        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "email@test.com", "ValidPass1!")]
    [InlineData("User", "", "ValidPass1!")]
    public async Task ShouldReturn400WhenMissingFields(string? name, string? email, string password)
    {
        var payload = new { Name = name, Email = email, Password = password };
        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("12345678")]
    [InlineData("Password123")]
    [InlineData("password!@#")]
    [InlineData("PASSWORD!@#")]
    public async Task ShouldReturn400WithInvalidPassword(string password)
    {
        var payload = new { Name = "User", Email = $"pwvalid-{Guid.NewGuid():N}@test.com", Password = password };
        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNormalizeEmailToLowercase()
    {
        var payload = new { Name = "Case User", Email = "CASE@Test.COM", Password = "ValidPass1!" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<Users>();
        user!.Email.Should().Be("case@test.com");
    }

    [Fact]
    public async Task ShouldReturn400WhenUserNameTooLong()
    {
        var longName = new string('A', 41);
        var payload = new { Name = longName, Email = $"long-{Guid.NewGuid():N}@test.com", Password = "ValidPass1!" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
