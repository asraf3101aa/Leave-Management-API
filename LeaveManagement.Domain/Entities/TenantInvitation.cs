using System;

namespace LeaveManagement.Domain.Entities;

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Expired = 3
}

public class TenantInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
