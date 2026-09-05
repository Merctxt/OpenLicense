namespace OpenLicense.Tests;

public static class TestExtensions
{
    public static async Task<HttpResponseMessage> DeleteAsJsonAsync(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(value));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = content };
        return await client.SendAsync(request, cancellationToken);
    }
}
