namespace OpenLicense.Tests.Features.Shared;

public class HealthTests
{
    [Fact]
    public void HealthEndpoint_ShouldReturnHealthy()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = client.GetAsync("/health").Result;

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class ExceptionHandlingTests
{
    [Fact]
    public async Task ShouldReturn404ForNonExistentEndpoint()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var response = await client.GetAsync("/api/nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn405ForMethodNotAllowed()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var response = await client.DeleteAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task ShouldReturn400ForInvalidJson()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = new StringContent("not valid json{{{")
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
