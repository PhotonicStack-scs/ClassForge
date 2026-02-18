using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClassForge.Application.DTOs.Auth;
using ClassForge.Application.DTOs.Timetables;
using ClassForge.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ClassForge.Tests.Integration.Endpoints;

public class TimetableEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TimetableEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

        var email = $"tt-{Guid.NewGuid()}@test.com";
        var response = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest("Timetable School", email, "Password123!", "TT User"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return (client, auth.AccessToken);
    }

    [Fact]
    public async Task Preflight_ReturnsValidResponse()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/v1/timetables/preflight", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PreflightResponse>();
        result.Should().NotBeNull();
        result!.Issues.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTimetable_Returns202()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Test Timetable"));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var timetable = await response.Content.ReadFromJsonAsync<TimetableResponse>();
        timetable!.Name.Should().Be("Test Timetable");
        timetable.Status.Should().Be("Generating");
    }

    [Fact]
    public async Task GetTimetable_AfterCreate_ReturnsIt()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Get Test"));
        var created = await createResponse.Content.ReadFromJsonAsync<TimetableResponse>();

        var getResponse = await client.GetAsync($"/api/v1/timetables/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListTimetables_ReturnsCreated()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/v1/timetables", new CreateTimetableRequest("List Test"));

        var response = await client.GetAsync("/api/v1/timetables");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<TimetableResponse>>();
        list.Should().Contain(t => t.Name == "List Test");
    }

    [Fact]
    public async Task DeleteTimetable_Returns204()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Delete Test"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var created = await createResponse.Content.ReadFromJsonAsync<TimetableResponse>();

        var deleteResponse = await client.DeleteAsync($"/api/v1/timetables/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
