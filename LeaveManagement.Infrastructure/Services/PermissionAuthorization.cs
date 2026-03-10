using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LeaveManagement.Infrastructure.Services;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public class PermissionHandler(ITenantService tenantService) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null) return Task.CompletedTask;

        // 1. SuperAdmin bypass platform-wide
        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Validate Tenant Scoping
        // Ensure the tenant the user is trying to access (from Header/Context) 
        // matches the tenant they are logged into (from Token).
        var effectiveTenantId = tenantService.GetTenantId();
        var tokenTenantId = context.User.FindFirstValue("tenant_id");

        if (!string.IsNullOrEmpty(effectiveTenantId) && effectiveTenantId != tokenTenantId)
        {
            // If they are trying to access a different tenant than what's in the token, fail.
            // (Unless they have a cross-tenant permission, but user didn't ask for that yet).
            return Task.CompletedTask;
        }

        // 3. Check for specific Permission claim
        var permissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value);

        if (permissions.Any(p => p == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public AuthorizePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        Task.FromResult(new AuthorizationPolicyBuilder("Bearer").RequireAuthenticatedUser().Build());

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Permissions"))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return Task.FromResult<AuthorizationPolicy?>(null);
    }
}
