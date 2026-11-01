namespace OpenLicense.Tests.Features.Reports;

public class GenerateReportTests : TestBase
{
    [Fact]
    public async Task ShouldReturnPdfWhenAuthenticated()
    {
        var (token, user) = await GetAuthenticatedUser("report1@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"{ApiVersion.Path}/reports/generate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var response = await Client.GetAsync($"{ApiVersion.Path}/reports/generate");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnPdfWhenNoProducts()
    {
        var (token, user) = await GetAuthenticatedUser("emptyreport@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"{ApiVersion.Path}/reports/generate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldReturnPdfWhenProductsExist()
    {
        var (token, user) = await GetAuthenticatedUser("report2@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create product and license
        var productResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/products/create", new { Name = "Test Product" });
        productResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();

        var licenseResponse = await Client.PostAsJsonAsync($"{ApiVersion.Path}/licenses", new { ProductId = product!.Id, Name = "License 1", MaxActivations = 1 });
        licenseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Generate report
        var response = await Client.GetAsync($"{ApiVersion.Path}/reports/generate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        pdfBytes.Should().NotBeNullOrEmpty();
    }
}
