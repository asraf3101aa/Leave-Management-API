using MediatR;
using LeaveManagement.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public class CreateLeaveRequestCommandHandler(ILeaveService leaveService)
    : IRequestHandler<CreateLeaveRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken) =>
        await leaveService.CreateLeaveRequest(
            request.StartDate,
            request.EndDate,
            request.LeaveTypeId,
            request.RequestComments,
            request.Duration,
            request.CreatedBy,
            cancellationToken
        );
}
