using Microsoft.AspNetCore.Authorization;

namespace LeaveManagement.Application.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
