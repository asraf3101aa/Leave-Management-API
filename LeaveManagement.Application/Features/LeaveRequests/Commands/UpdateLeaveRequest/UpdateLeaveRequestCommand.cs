using MediatR;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public record UpdateLeaveRequestCommand(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    Guid LeaveTypeId,
    string RequestComments,
    LeaveDuration Duration,
    Guid UpdatedBy
) : IRequest;
