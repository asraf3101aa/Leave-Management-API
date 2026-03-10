using Microsoft.AspNetCore.Identity;
using LeaveManagement.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Services;

public interface IAuthService
{
    Task<string> Login(string email, string password, string? tenantId = null);
    Task<bool> Register(ApplicationUser user, string password, string? roleName = null, string? tenantId = null, string? changedBy = null);
    Task<bool> AssignRoleToUserAsync(string userId, string roleName, string? tenantId, string changedBy);
    Task<bool> CreateRole(string roleName, string? tenantId, string changedBy);
    Task<IEnumerable<string>> GetRolesByTenant(string? tenantId);
    Task<bool> SoftDeleteUser(string userId);
    Task<bool> SoftDeleteRole(string roleName, string? tenantId, string changedBy);
    Task<bool> ToggleUserActive(string userId);
    Task<bool> CreateInvitation(string email, string tenantId, string roleName, string createdBy);
    Task<TenantInvitation?> GetInvitationByToken(string token);
    Task<bool> AcceptInvitation(string token, string password, string firstName, string lastName);
    Task<bool> SetDefaultRole(string tenantId, string roleName, string changedBy);
    Task<bool> UpsertRolePermissions(string roleName, string? tenantId, List<string> permissions, string changedBy);
    Task<List<string>> GetPermissionsByRole(string roleName, string? tenantId);
}

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<ApplicationRole> roleManager,
    MasterDbContext masterContext,
    IApplicationDbContext tenantContext,
    IConfiguration configuration) : IAuthService
{
    public async Task<string> Login(string email, string password, string? tenantId = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted || !user.IsActive) return string.Empty;

        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenant = await masterContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
            if (tenant == null || !tenant.IsActive || tenant.IsDeleted) return string.Empty;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return string.Empty;

        return await GenerateToken(user, tenantId);
    }

    public async Task<bool> Register(ApplicationUser user, string password, string? roleName = null, string? tenantId = null, string? changedBy = null)
    {
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return false;

        string? finalRole = roleName;
        if (string.IsNullOrEmpty(finalRole) && !string.IsNullOrEmpty(tenantId))
        {
            var tenant = await masterContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
            finalRole = tenant?.DefaultRoleName;
        }

        if (!string.IsNullOrEmpty(finalRole))
        {
            await AssignRoleToUserAsync(user.Id, finalRole, tenantId, changedBy ?? "System");
        }

        return true;
    }

    public async Task<bool> SetDefaultRole(string tenantId, string roleName, string changedBy)
    {
        var tenant = await masterContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null) return false;

        tenant.DefaultRoleName = roleName;
        tenant.UpdatedBy = changedBy;
        tenant.UpdatedAt = DateTime.UtcNow;

        masterContext.TenantHistories.Add(new TenantHistory
        {
            TenantId = tenantId,
            Action = "DefaultRoleChanged",
            Details = $"Default role for tenant changed to {roleName}",
            ChangedBy = changedBy
        });

        return await masterContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpsertRolePermissions(string roleName, string? tenantId, List<string> permissions, string changedBy)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null) return false;

        var existingClaims = await masterContext.RoleClaims
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "Permission")
            .ToListAsync();

        // Soft delete removed permissions
        foreach (var claim in existingClaims)
        {
            if (!permissions.Contains(claim.ClaimValue!))
            {
                claim.IsDeleted = true;
                claim.DeletedAt = DateTime.UtcNow;
            }
        }

        // Add new permissions
        foreach (var p in permissions)
        {
            var existing = existingClaims.FirstOrDefault(c => c.ClaimValue == p);
            if (existing == null)
            {
                masterContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = "Permission",
                    ClaimValue = p
                });
            }
            else if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
            }
        }

        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantContext.RoleHistories.Add(new RoleHistory
            {
                Action = "RolePermissionsUpdated",
                EntityId = role.Id,
                EntityName = roleName,
                ChangedBy = changedBy,
                Details = $"Updated permissions for role {roleName}"
            });
            await tenantContext.SaveChangesAsync();
        }

        return await masterContext.SaveChangesAsync() > 0;
    }

    public async Task<List<string>> GetPermissionsByRole(string roleName, string? tenantId)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null) return new List<string>();

        return await masterContext.RoleClaims
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "Permission")
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();
    }

    public async Task<bool> CreateInvitation(string email, string tenantId, string roleName, string createdBy)
    {
        var invitation = new TenantInvitation
        {
            Id = Guid.NewGuid(),
            Email = email,
            TenantId = tenantId,
            RoleName = roleName,
            Token = Guid.NewGuid().ToString("N"),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedBy = createdBy
        };

        masterContext.TenantInvitations.Add(invitation);
        await masterContext.SaveChangesAsync();
        return true;
    }

    public async Task<TenantInvitation?> GetInvitationByToken(string token)
    {
        return await masterContext.TenantInvitations
            .FirstOrDefaultAsync(i => i.Token == token && i.Status == InvitationStatus.Pending && i.ExpiryDate > DateTime.UtcNow);
    }

    public async Task<bool> AcceptInvitation(string token, string password, string firstName, string lastName)
    {
        var invitation = await GetInvitationByToken(token);
        if (invitation == null) return false;

        var user = new ApplicationUser
        {
            UserName = invitation.Email,
            Email = invitation.Email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await Register(user, password, invitation.RoleName, invitation.TenantId, "InvitationSystem");
        if (result)
        {
            invitation.Status = InvitationStatus.Accepted;
            await masterContext.SaveChangesAsync();
        }

        return result;
    }

    public async Task<bool> SoftDeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> SoftDeleteRole(string roleName, string? tenantId, string changedBy)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null) return false;

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.UpdatedBy = changedBy;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await roleManager.UpdateAsync(role);
        if (result.Succeeded && !string.IsNullOrEmpty(tenantId))
        {
            tenantContext.RoleHistories.Add(new RoleHistory
            {
                Action = "DeletedRole",
                EntityId = role.Id,
                EntityName = roleName,
                ChangedBy = changedBy,
                Details = $"Soft deleted role {roleName}"
            });
            await tenantContext.SaveChangesAsync();
        }

        return result.Succeeded;
    }

    public async Task<bool> ToggleUserActive(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName, string? tenantId, string changedBy)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null)
        {
            var createResult = await CreateRole(roleName, tenantId, changedBy);
            if (!createResult) return false;
            role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        }

        if (role == null) return false;

        var existingAssignment = await masterContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.RoleId == role.Id);
        if (existingAssignment != null)
        {
            if (existingAssignment.IsDeleted)
            {
                existingAssignment.IsDeleted = false;
                existingAssignment.DeletedAt = null;
            }
            else
            {
                return true;
            }
        }
        else
        {
            masterContext.UserRoles.Add(new TenantUserRole
            {
                UserId = userId,
                RoleId = role.Id,
                TenantId = tenantId,
                CreatedBy = changedBy,
                CreatedAt = DateTime.UtcNow
            });
        }

        await masterContext.SaveChangesAsync();

        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantContext.RoleHistories.Add(new RoleHistory
            {
                Action = "Assigned",
                EntityId = userId,
                EntityName = roleName,
                ChangedBy = changedBy,
                Details = $"Assigned role {roleName} to user {userId}"
            });
            await tenantContext.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> CreateRole(string roleName, string? tenantId, string changedBy)
    {
        if (await roleManager.Roles.AnyAsync(r => r.Name == roleName && r.TenantId == tenantId)) return false;

        var role = new ApplicationRole(roleName, tenantId)
        {
            CreatedBy = changedBy,
            CreatedAt = DateTime.UtcNow
        };

        var result = await roleManager.CreateAsync(role);
        if (result.Succeeded && !string.IsNullOrEmpty(tenantId))
        {
            tenantContext.RoleHistories.Add(new RoleHistory
            {
                Action = "Created",
                EntityId = role.Id,
                EntityName = roleName,
                ChangedBy = changedBy,
                Details = $"Created role {roleName}"
            });
            await tenantContext.SaveChangesAsync();
        }

        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetRolesByTenant(string? tenantId)
    {
        return await roleManager.Roles
            .Where(r => r.TenantId == tenantId || r.TenantId == null)
            .Select(r => r.Name!)
            .ToListAsync();
    }

    private async Task<string> GenerateToken(ApplicationUser user, string? tenantId)
    {
        var rolesWithPermissions = await masterContext.UserRoles
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .Where(ur => string.IsNullOrEmpty(tenantId) ? ur.TenantId == null : (ur.TenantId == tenantId || ur.TenantId == null))
            .Join(masterContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Id, r.Name })
            .ToListAsync();

        var permissions = await masterContext.RoleClaims
            .Where(rc => rc.ClaimType == "Permission" && rolesWithPermissions.Select(r => r.Id).Contains(rc.RoleId))
            .Select(rc => rc.ClaimValue!)
            .Distinct()
            .ToListAsync();

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", user.Id),
            new Claim("tenant_id", tenantId ?? string.Empty)
        };

        foreach (var r in rolesWithPermissions)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, r.Name!));
        }

        foreach (var p in permissions)
        {
            authClaims.Add(new Claim("Permission", p));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? "DefaultSecretKeyPlaceholderLongEnough"));

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
