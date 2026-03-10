using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Domain.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationRole : IdentityRole
{
    public string? TenantId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete for Roles
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName, string? tenantId) : base(roleName)
    {
        TenantId = tenantId;
    }
}

public class TenantUserRole : IdentityUserRole<string>
{
    public string? TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
