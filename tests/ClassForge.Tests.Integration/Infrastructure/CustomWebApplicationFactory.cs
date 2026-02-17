using ClassForge.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ClassForge.Tests.Integration.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            // Remove the hosted service to avoid background generation during tests
            var hostedDescriptor = services.SingleOrDefault(d =>
                d.ImplementationType == typeof(ClassForge.Infrastructure.Services.TimetableGenerationService));
            if (hostedDescriptor is not null) services.Remove(hostedDescriptor);

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.AddInterceptors(sp.GetRequiredService<TenantInterceptor>());
            });
        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public async Task<AppDbContext> CreateDbContextAsync()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
