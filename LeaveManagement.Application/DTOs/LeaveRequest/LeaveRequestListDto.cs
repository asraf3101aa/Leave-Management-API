using LeaveManagement.Domain.Entities;
namespace LeaveManagement.Application.DTOs.LeaveRequest;

public record LeaveRequestListDto(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    Guid LeaveTypeId,
    LeaveStatus Status,
    LeaveDuration Duration,
    Guid CreatedBy
);
