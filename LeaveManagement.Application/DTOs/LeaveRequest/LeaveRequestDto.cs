using LeaveManagement.Domain.Entities;
namespace LeaveManagement.Application.DTOs.LeaveRequest;

public record LeaveRequestDto(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    Guid LeaveTypeId,
    DateTime CreatedAt,
    Guid CreatedBy,
    string RequestComments,
    LeaveStatus Status,
    LeaveDuration Duration,
    Guid? ApprovedBy,
    Guid? ReviewedBy,
    DateTime? ApprovedAt,
    DateTime? ReviewedAt
);
