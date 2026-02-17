using ClassForge.Application.Interfaces;
using ClassForge.Domain.Entities;
using ClassForge.Infrastructure.Data;
using ClassForge.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<TenantInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<TenantInterceptor>());
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPreflightValidator, PreflightValidator>();
        services.AddScoped<ITimetableEntryValidator, TimetableEntryValidator>();
        services.AddScoped<SchedulingInputBuilder>();
        services.AddSingleton<TimetableGenerationQueue>();
        services.AddSingleton<ITimetableGenerationQueue>(sp => sp.GetRequiredService<TimetableGenerationQueue>());
        services.AddHostedService<TimetableGenerationService>();

        return services;
    }
}
