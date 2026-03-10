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
    public string TenantId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
