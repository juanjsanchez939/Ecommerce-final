using System.Net.Http.Json;

namespace Ecommerce.IntegrationTests.Products;

public class ProductsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Test",
            description = "Test",
            price = 100,
            stock = 10,
            imageUrl = "https://img.jpg",
            categoryId = Guid.NewGuid()
        });

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_NotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
