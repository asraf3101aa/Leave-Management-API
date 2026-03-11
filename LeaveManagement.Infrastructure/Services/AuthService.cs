using Microsoft.AspNetCore.Identity;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Application.Models.Email;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<ApplicationRole> roleManager,
    MasterDbContext masterContext,
    IConfiguration configuration,
    IEmailQueuePublisher emailQueuePublisher) : IAuthService
{
    public async Task<AuthResponse?> Login(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted || !user.IsActive) return null;

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return null;

        return await GenerateAuthResponse(user);
    }

    public async Task<bool> Register(string email, string password, string firstName, string lastName, string? roleName = null, Guid? tenantId = null, Guid? changedBy = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return false;

        string? finalRole = roleName;
        if (string.IsNullOrEmpty(finalRole) && tenantId.HasValue)
        {
            var tenant = await masterContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId.Value);
            finalRole = tenant?.DefaultRoleName;
        }

        if (!string.IsNullOrEmpty(finalRole))
        {
            await AssignRoleToUserAsync(user.Id, finalRole, tenantId, changedBy ?? Guid.Empty);
        }

        return true;
    }

    public async Task<bool> SetDefaultRole(Guid tenantId, string roleName, Guid changedBy)
    {
        var tenant = await masterContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null) return false;

        tenant.DefaultRoleName = roleName;
        tenant.UpdatedBy = changedBy;
        tenant.UpdatedAt = DateTime.UtcNow;

        return await masterContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpsertRolePermissions(string roleName, Guid? tenantId, List<string> permissions, Guid changedBy)
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



        return await masterContext.SaveChangesAsync() > 0;
    }

    public async Task<List<string>> GetPermissionsByRole(string roleName, Guid? tenantId)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null) return new List<string>();

        return await masterContext.RoleClaims
            .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "Permission")
            .Select(rc => rc.ClaimValue!)
            .ToListAsync();
    }

    public async Task<bool> CreateInvitation(string email, Guid tenantId, string roleName, Guid createdBy)
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

        var emailMsg = new EmailMessage
        {
            To = email,
            Subject = "You've been invited!",
            Body = $"You've been invited to join the Leave Management platform as {roleName}. Your token is: {invitation.Token}"
        };
        await emailQueuePublisher.PublishEmailAsync(emailMsg);

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

        var result = await Register(invitation.Email, password, firstName, lastName, invitation.RoleName, invitation.TenantId, Guid.Empty);
        if (result)
        {
            invitation.Status = InvitationStatus.Accepted;
            await masterContext.SaveChangesAsync();
        }

        return result;
    }

    public async Task<bool> SoftDeleteUser(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> SoftDeleteRole(string roleName, Guid? tenantId, Guid changedBy)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        if (role == null) return false;

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.UpdatedBy = changedBy;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await roleManager.UpdateAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> ToggleUserActive(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName, Guid? tenantId, Guid changedBy)
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



        return true;
    }

    public async Task<bool> CreateRole(string roleName, Guid? tenantId, Guid changedBy)
    {
        if (await roleManager.Roles.AnyAsync(r => r.Name == roleName && r.TenantId == tenantId)) return false;

        var role = new ApplicationRole(roleName, tenantId)
        {
            CreatedBy = changedBy,
            CreatedAt = DateTime.UtcNow
        };

        var result = await roleManager.CreateAsync(role);


        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetRolesByTenant(Guid? tenantId)
    {
        return await roleManager.Roles
            .Where(r => r.TenantId == tenantId || r.TenantId == null)
            .Select(r => r.Name!)
            .ToListAsync();
    }

    public async Task<AuthResponse?> Refresh(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:RefreshKey"] ?? (configuration["JwtSettings:Key"] + "RefreshSecret")));

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = key
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            var userType = principal.FindFirstValue("type");
            if (userType != "refresh") return null;

            var userId = principal.FindFirstValue("userId");

            if (string.IsNullOrEmpty(userId)) return null;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted || !user.IsActive) return null;

            return await GenerateAuthResponse(user);
        }
        catch
        {
            return null;
        }
    }

    private async Task<AuthResponse> GenerateAuthResponse(ApplicationUser user)
    {
        var rolesWithPermissions = await masterContext.UserRoles
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .Join(masterContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { r.Id, r.Name })
            .Distinct()
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
            new Claim("userId", user.Id.ToString())
        };

        foreach (var r in rolesWithPermissions)
            authClaims.Add(new Claim(ClaimTypes.Role, r.Name!));

        foreach (var p in permissions)
            authClaims.Add(new Claim("Permission", p));

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? "DefaultSecretKeyPlaceholderLongEnough"));

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            expires: DateTime.UtcNow.AddHours(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        var refreshClaims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("type", "refresh")
        };

        var refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:RefreshKey"] ?? (configuration["JwtSettings:Key"] + "RefreshSecret")));
        var refreshTokenObj = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            expires: refreshTokenExpiry,
            claims: refreshClaims,
            signingCredentials: new SigningCredentials(refreshKey, SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshTokenObj);

        return new AuthResponse(accessToken, refreshToken, refreshTokenExpiry);
    }

    public async Task<List<Guid>> GetUserTenantIds(Guid userId)
    {
        return await masterContext.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted && ur.TenantId != null)
            .Select(ur => ur.TenantId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> IsUserInTenant(Guid userId, Guid tenantId)
    {
        return await masterContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.TenantId == tenantId && !ur.IsDeleted);
    }
}
