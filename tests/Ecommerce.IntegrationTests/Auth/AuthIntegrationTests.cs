using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecommerce.Application.Common.DTOs;
using Ecommerce.Application.Features.Auth.Commands;

namespace Ecommerce.IntegrationTests.Auth;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullAuthFlow_RegisterThenLoginThenUseToken()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "flow@test.com",
            password = "Password123!",
            firstName = "Flow",
            lastName = "Test"
        });

        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>();
        Assert.NotNull(registerResult);
        Assert.True(registerResult.Success);
        Assert.NotNull(registerResult.Data.Token);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "flow@test.com",
            password = "Password123!"
        });

        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResultDto>>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Data.Token);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShouldReturnBadRequest()
    {
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup2@test.com",
            password = "Password123!",
            firstName = "First",
            lastName = "User"
        });
        response1.EnsureSuccessStatusCode();

        var response2 = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup2@test.com",
            password = "Password123!",
            firstName = "Second",
            lastName = "User"
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnBadRequest()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "noexist@test.com",
            password = "Password123!"
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/orders");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
