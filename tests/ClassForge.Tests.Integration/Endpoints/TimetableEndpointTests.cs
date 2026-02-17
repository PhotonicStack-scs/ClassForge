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
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TimetableEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var db = await _factory.CreateDbContextAsync();
        var email = $"tt-{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest("Timetable School", email, "Password123!", "TT User"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    [Fact]
    public async Task Preflight_ReturnsValidResponse()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/v1/timetables/preflight", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PreflightResponse>();
        result.Should().NotBeNull();
        result!.Issues.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTimetable_Returns202()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Test Timetable"));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var timetable = await response.Content.ReadFromJsonAsync<TimetableResponse>();
        timetable!.Name.Should().Be("Test Timetable");
        timetable.Status.Should().Be("Generating");
    }

    [Fact]
    public async Task GetTimetable_AfterCreate_ReturnsIt()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Get Test"));
        var created = await createResponse.Content.ReadFromJsonAsync<TimetableResponse>();

        var getResponse = await _client.GetAsync($"/api/v1/timetables/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListTimetables_ReturnsCreated()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/timetables", new CreateTimetableRequest("List Test"));

        var response = await _client.GetAsync("/api/v1/timetables");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<TimetableResponse>>();
        list.Should().Contain(t => t.Name == "List Test");
    }

    [Fact]
    public async Task DeleteTimetable_Returns204()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/timetables",
            new CreateTimetableRequest("Delete Test"));
        var created = await createResponse.Content.ReadFromJsonAsync<TimetableResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/timetables/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
