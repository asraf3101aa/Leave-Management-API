using LeaveManagement.Domain.Entities;
namespace LeaveManagement.Application.DTOs.LeaveRequest;

public record LeaveRequestDto(
    Guid Id,
    string RequestingEmployeeId,
    DateTime StartDate,
    DateTime EndDate,
    int LeaveTypeId,
    DateTime DateRequested,
    string RequestComments,
    LeaveStatus Status,
    bool Cancelled,
    LeaveDuration Duration,
    string? ApprovedBy,
    string? UpdatedBy,
    DateTime? UpdatedAt
);
