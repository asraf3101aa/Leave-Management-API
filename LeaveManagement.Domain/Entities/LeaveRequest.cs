using System;

namespace LeaveManagement.Domain.Entities;

public enum LeaveDuration
{
    FullDay = 1,
    HalfDay = 2
}

public enum LeaveStatus
{
    Pending = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4
}

public class LeaveRequest
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public string RequestComments { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public LeaveDuration Duration { get; set; } = LeaveDuration.FullDay;

    public Guid? ApprovedBy { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
