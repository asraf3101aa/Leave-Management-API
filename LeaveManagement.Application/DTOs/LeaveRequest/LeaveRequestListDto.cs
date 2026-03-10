using LeaveManagement.Domain.Entities;
namespace LeaveManagement.Application.DTOs.LeaveRequest;

public record LeaveRequestListDto(
    Guid Id,
    string RequestingEmployeeId,
    DateTime StartDate,
    DateTime EndDate,
    int LeaveTypeId,
    LeaveStatus Status,
    LeaveDuration Duration
);
