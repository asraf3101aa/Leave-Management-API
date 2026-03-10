using System;

namespace LeaveManagement.Domain.Entities;

public class RoleHistory
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Assigned", "Removed"
    public string? EntityId { get; set; } // RoleId or UserId
    public string? EntityName { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
