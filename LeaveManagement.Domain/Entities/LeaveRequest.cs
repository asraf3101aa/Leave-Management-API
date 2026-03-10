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
    public string RequestingEmployeeId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime DateRequested { get; set; }
    public string RequestComments { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public bool Cancelled { get; set; }
    public LeaveDuration Duration { get; set; } = LeaveDuration.FullDay;

    // Tracking/Audit
    public string? ApprovedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
