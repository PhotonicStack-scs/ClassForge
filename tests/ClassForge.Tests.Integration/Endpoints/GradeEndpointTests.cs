using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClassForge.Application.DTOs.Auth;
using ClassForge.Application.DTOs.Grades;
using ClassForge.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace ClassForge.Tests.Integration.Endpoints;

public class GradeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GradeEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync(string? email = null)
    {
        var db = await _factory.CreateDbContextAsync();
        email ??= $"grade-test-{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest("Grade School", email, "Password123!", "Grade User"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    [Fact]
    public async Task CRUD_Grade_Succeeds()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/grades",
            new CreateGradeRequest("Grade 8", 8));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var grade = await createResponse.Content.ReadFromJsonAsync<GradeResponse>();
        grade!.Name.Should().Be("Grade 8");

        // Get by ID
        var getResponse = await _client.GetAsync($"/api/v1/grades/{grade.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/grades/{grade.Id}",
            new UpdateGradeRequest("Grade 8 Updated", 8));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<GradeResponse>();
        updated!.Name.Should().Be("Grade 8 Updated");

        // List
        var listResponse = await _client.GetAsync("/api/v1/grades");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/v1/grades/{grade.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TenantIsolation_DifferentTenants_CantSeeEachOther()
    {
        // Tenant 1
        var token1 = await GetTokenAsync($"tenant1-{Guid.NewGuid()}@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        await _client.PostAsJsonAsync("/api/v1/grades", new CreateGradeRequest("Isolated Grade", 1));

        // Tenant 2
        var token2 = await GetTokenAsync($"tenant2-{Guid.NewGuid()}@test.com");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        var response = await _client.GetAsync("/api/v1/grades");
        var grades = await response.Content.ReadFromJsonAsync<List<GradeResponse>>();
        grades.Should().NotContain(g => g.Name == "Isolated Grade");
    }
}
