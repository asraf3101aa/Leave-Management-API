using LeaveManagement.Application.Constants;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        try
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<MasterDbContext>();

            // Ensure DB is created
            await context.Database.MigrateAsync();

            // Seed SuperAdmin User
            var superAdminEmail = "admin@leaveflow.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin == null)
            {
                logger.LogInformation("Seeding SuperAdmin User...");
                superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to create SuperAdmin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            // Seed SuperAdmin Role
            if (!await roleManager.RoleExistsAsync(Roles.SuperAdmin))
            {
                logger.LogInformation("Seeding SuperAdmin Role...");
                var role = new ApplicationRole(Roles.SuperAdmin, null)
                {
                    CreatedBy = superAdmin.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await roleManager.CreateAsync(role);

                // Add all permissions to SuperAdmin role
                var allPermissions = Permissions.GetAll();
                foreach (var permission in allPermissions)
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permission));
                }
            }

            // Ensure SuperAdmin user has the Role
            var adminRole = await roleManager.FindByNameAsync(Roles.SuperAdmin);
            if (adminRole != null)
            {
                var roleId = adminRole.Id;
                var hasRole = await context.UserRoles.AnyAsync(ur => ur.UserId == superAdmin.Id && ur.RoleId == roleId && ur.TenantId == null);
                if (!hasRole)
                {
                    logger.LogInformation("Assigning SuperAdmin Role to User...");
                    context.UserRoles.Add(new TenantUserRole
                    {
                        UserId = superAdmin.Id,
                        RoleId = roleId,
                        TenantId = null,
                        CreatedAt = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Master Database.");
        }
    }
}
