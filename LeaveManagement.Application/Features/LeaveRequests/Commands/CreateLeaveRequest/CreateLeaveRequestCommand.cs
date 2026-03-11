using MediatR;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public record CreateLeaveRequestCommand(
    DateTime StartDate,
    DateTime EndDate,
    Guid LeaveTypeId,
    string RequestComments,
    Guid CreatedBy,
    LeaveDuration Duration = LeaveDuration.FullDay
) : IRequest<Guid>;
