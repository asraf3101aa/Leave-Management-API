using System;

namespace LeaveManagement.Domain.Entities;

public class Tenant
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? DefaultRoleName { get; set; }

    // Auditing
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class TenantHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Deleted", "Activated", "Deactivated", "DefaultRoleChanged"
    public string Details { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
