using MediatR;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public record CreateLeaveRequestCommand(
    string RequestingEmployeeId,
    DateTime StartDate,
    DateTime EndDate,
    int LeaveTypeId,
    string RequestComments,
    LeaveDuration Duration = LeaveDuration.FullDay
) : IRequest<Guid>;
