namespace OpenLicense.Tests.Features.Products;

public class CreateProductTests : TestBase
{
    [Fact]
    public async Task ShouldCreateProductWithValidData()
    {
        var (token, user) = await GetAuthenticatedUser("createprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { Name = "My Product" };
        var response = await Client.PostAsJsonAsync("/api/products/create", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("My Product");
        product.UserId.Should().Be(user.Id);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var payload = new { Name = "Hacker Product" };
        var response = await Client.PostAsJsonAsync("/api/products/create", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldRespectProductLimit()
    {
        var (token, user) = await GetAuthenticatedUser($"limitprod-{Guid.NewGuid():N}@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (int i = 0; i < 5; i++)
        {
            var payload = new { Name = $"Product {i}" };
            var response = await Client.PostAsJsonAsync("/api/products/create", payload);
            if (i < 3)
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            else
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}

public class GetProductsTests : TestBase
{
    [Fact]
    public async Task ShouldReturnProductsForUser()
    {
        var (token, user) = await GetAuthenticatedUser("getprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await CreateProduct(token, "Product 1");
        await CreateProduct(token, "Product 2");

        var response = await Client.GetAsync("/api/products/all");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
        products!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenNoProducts()
    {
        var (token, user) = await GetAuthenticatedUser("emptyprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/products/all");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/api/products/all");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task CreateProduct(string token, string name)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = new { Name = name };
        var response = await Client.PostAsJsonAsync("/api/products/create", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class UpdateProductTests : TestBase
{
    [Fact]
    public async Task ShouldUpdateProductWhenOwned()
    {
        var (token, user) = await GetAuthenticatedUser("updateprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = "Old Name" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        var updatePayload = new { ProductId = product!.Id, Name = "New Name" };
        var updateResponse = await Client.PutAsJsonAsync("/api/products/update", updatePayload);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<Product>();
        updated!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task ShouldReturn404WhenProductNotFound()
    {
        var (token, user) = await GetAuthenticatedUser("notfoundprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = Guid.NewGuid(), Name = "Ghost" };
        var response = await Client.PutAsJsonAsync("/api/products/update", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenProductBelongsToAnotherUser()
    {
        var (token1, user1) = await GetAuthenticatedUser("ownerprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = "Owner Product" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        var (token2, user2) = await GetAuthenticatedUser("otherprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var payload = new { ProductId = product!.Id, Name = "Hacked" };
        var response = await Client.PutAsJsonAsync("/api/products/update", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var payload = new { ProductId = Guid.NewGuid(), Name = "Hacked" };
        var response = await Client.PutAsJsonAsync("/api/products/update", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class DeleteProductTests : TestBase
{
    [Fact]
    public async Task ShouldDeleteProductWhenOwned()
    {
        var (token, user) = await GetAuthenticatedUser("delprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = "Delete Me" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        var deletePayload = new { ProductId = product!.Id };
        var deleteResponse = await Client.DeleteAsJsonAsync("/api/products", deletePayload);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var allResponse = await Client.GetAsync("/api/products/all");
        var products = await allResponse.Content.ReadFromJsonAsync<List<Product>>();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturn404WhenProductNotFound()
    {
        var (token, user) = await GetAuthenticatedUser("notfounddel@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { ProductId = Guid.NewGuid() };
        var response = await Client.DeleteAsJsonAsync("/api/products", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn404WhenProductBelongsToAnotherUser()
    {
        var (token1, user1) = await GetAuthenticatedUser("ownerdelprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await Client.PostAsJsonAsync("/api/products/create", new { Name = "Owner Product" });
        var product = await createResponse.Content.ReadFromJsonAsync<Product>();

        var (token2, user2) = await GetAuthenticatedUser("otherdelprod@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var payload = new { ProductId = product!.Id };
        var response = await Client.DeleteAsJsonAsync("/api/products", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturn401WhenNotAuthenticated()
    {
        var payload = new { ProductId = Guid.NewGuid() };
        var response = await Client.DeleteAsJsonAsync("/api/products", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
