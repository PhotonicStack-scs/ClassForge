using System.Net;
using System.Net.Http.Json;
using ClassForge.Application.DTOs.Auth;
using ClassForge.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ClassForge.Tests.Integration.Endpoints;

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

    [Fact]
    public async Task Register_ReturnsTokens()
    {
        var client = CreateClient();
        var request = new RegisterRequest("Test School", $"test-{Guid.NewGuid()}@test.com", "Password123!", "Test User");
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
        auth.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var client = CreateClient();
        var email = $"login-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest("Login School", email, "Password123!", "Login User"));

        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/grades");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
