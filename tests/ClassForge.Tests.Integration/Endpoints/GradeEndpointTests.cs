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
    private readonly CustomWebApplicationFactory _factory;

    public GradeEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string? email = null)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false,
            AllowAutoRedirect = false
        });

        email ??= $"grade-test-{Guid.NewGuid()}@test.com";
        var response = await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest("Grade School", email, "Password123!", "Grade User"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return client;
    }

    [Fact]
    public async Task CRUD_Grade_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Create
        var createResponse = await client.PostAsJsonAsync("/api/v1/grades",
            new CreateGradeRequest("Grade 8", 8));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var grade = await createResponse.Content.ReadFromJsonAsync<GradeResponse>();
        grade!.Name.Should().Be("Grade 8");

        // Get by ID
        var getResponse = await client.GetAsync($"/api/v1/grades/{grade.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/grades/{grade.Id}",
            new UpdateGradeRequest("Grade 8 Updated", 8));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<GradeResponse>();
        updated!.Name.Should().Be("Grade 8 Updated");

        // List
        var listResponse = await client.GetAsync("/api/v1/grades");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/v1/grades/{grade.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TenantIsolation_DifferentTenants_CantSeeEachOther()
    {
        // Tenant 1
        var client1 = await CreateAuthenticatedClientAsync($"tenant1-{Guid.NewGuid()}@test.com");
        await client1.PostAsJsonAsync("/api/v1/grades", new CreateGradeRequest("Isolated Grade", 1));

        // Tenant 2
        var client2 = await CreateAuthenticatedClientAsync($"tenant2-{Guid.NewGuid()}@test.com");

        var response = await client2.GetAsync("/api/v1/grades");
        var grades = await response.Content.ReadFromJsonAsync<List<GradeResponse>>();
        grades.Should().NotContain(g => g.Name == "Isolated Grade");
    }
}
