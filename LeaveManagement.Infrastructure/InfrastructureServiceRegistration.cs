using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Infrastructure.Messaging;
using LeaveManagement.Infrastructure.Persistence;
using LeaveManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagement.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Master Database (Users, Tenants, Roles)
        services.AddDbContext<MasterDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMasterDbContext>(provider => provider.GetRequiredService<MasterDbContext>());

        // Tenant Database (Leave Requests, Audit Logs) - Dynamic Connection
        services.AddDbContext<TenantDbContext>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TenantDbContext>());

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<MasterDbContext>()
            .AddDefaultTokenProviders();

        // Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILeaveService, LeaveService>();

        // Messaging & Email
        services.AddScoped<IEmailSender, MockEmailSender>();
        services.AddSingleton<IEmailQueuePublisher, RabbitMqEmailPublisher>();
        services.AddHostedService<EmailQueueConsumerService>();

        // RBAC handler
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        return services;
    }
}
