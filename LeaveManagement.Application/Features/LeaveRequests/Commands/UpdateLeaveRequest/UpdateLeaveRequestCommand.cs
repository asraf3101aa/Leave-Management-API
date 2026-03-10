using MediatR;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public record UpdateLeaveRequestCommand(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    int LeaveTypeId,
    string RequestComments,
    LeaveStatus Status,
    bool Cancelled,
    LeaveDuration Duration,
    string UpdatedBy
) : IRequest;
