using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationRole : IdentityRole<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName, Guid? tenantId) : base(roleName)
    {
        TenantId = tenantId;
    }
}

public class TenantUserRole : IdentityUserRole<Guid>
{
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
