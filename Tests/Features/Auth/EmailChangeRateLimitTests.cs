namespace OpenLicense.Tests.Features.Auth;

public record ErrorMessage(string Message);

public class EmailChangeRateLimitTests : TestBase
{
    [Fact]
    public async Task ShouldBlockEmailChangeWhenLastChangeWithin3Months()
    {
        var (token, user) = await GetAuthenticatedUser("ratelimit@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload1 = new { Email = "ratelimit2@test.com" };
        var response1 = await Client.PutAsJsonAsync("/api/auth", payload1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload2 = new { Email = "ratelimit3@test.com" };
        var response2 = await Client.PutAsJsonAsync("/api/auth", payload2);

        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response2.Content.ReadFromJsonAsync<ErrorMessage>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("Maximum number of email changes");
    }

    [Fact]
    public async Task ShouldAllowEmailChangeWhenLastChangeOlderThan3Months()
    {
        var (token, user) = await GetAuthenticatedUser("oldchange@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "oldchange@test.com");
        dbUser!.LastEmailChangeAt = DateTime.UtcNow.AddMonths(-4);
        await dbContext.SaveChangesAsync();

        var payload = new { Email = "oldchange2@test.com" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Users>();
        updated!.Email.Should().Be("oldchange2@test.com");
        updated.LastEmailChangeAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldNotBlockWhenOnlyUpdatingNameWithSameEmail()
    {
        var (token, user) = await GetAuthenticatedUser("namely@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload1 = new { Email = "namely2@test.com" };
        var response1 = await Client.PutAsJsonAsync("/api/auth", payload1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "namely2@test.com");
        dbUser!.LastEmailChangeAt.Should().NotBeNull();

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload2 = new { Name = "New Name", Email = "namely2@test.com" };
        var response2 = await Client.PutAsJsonAsync("/api/auth", payload2);

        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response2.Content.ReadFromJsonAsync<Users>();
        updated!.Name.Should().Be("New Name");
        updated.Email.Should().Be("namely2@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload3 = new { Email = "namely3@test.com" };
        var response3 = await Client.PutAsJsonAsync("/api/auth", payload3);

        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotBlockWhenEmailDoesNotChange()
    {
        var (token, user) = await GetAuthenticatedUser("sameemail@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Name = "New Name" };
        var response = await Client.PutAsJsonAsync("/api/auth", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Users>();
        updated!.Name.Should().Be("New Name");
        updated.Email.Should().Be("sameemail@test.com");
    }
}
