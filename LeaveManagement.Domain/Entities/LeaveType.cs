using System;

namespace LeaveManagement.Domain.Entities;

public class LeaveType
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DefaultDays { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
