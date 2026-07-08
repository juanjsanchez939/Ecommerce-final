using System.Net.Http.Json;
using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Features.Auth.Commands;

namespace Ecommerce.IntegrationTests.Orders;

public class OrdersIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAndLogin_WorksInOrdersContext()
    {
        var client = _factory.CreateClient();

        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "ordertest@e2e.com",
            password = "Order123!",
            firstName = "Order",
            lastName = "Tester"
        });
        regResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "ordertest@e2e.com",
            password = "Order123!"
        });
        loginResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetOrders_WithoutAuth_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/orders");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
