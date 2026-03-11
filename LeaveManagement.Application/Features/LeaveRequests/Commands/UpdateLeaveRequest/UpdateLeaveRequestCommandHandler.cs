using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public class UpdateLeaveRequestCommandHandler(ILeaveService leaveService)
    : IRequestHandler<UpdateLeaveRequestCommand>
{
    public async Task Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken) =>
        await leaveService.UpdateLeaveRequest(
            request.Id,
            request.StartDate,
            request.EndDate,
            request.LeaveTypeId,
            request.RequestComments,
            request.Duration,
            request.UpdatedBy,
            cancellationToken
        );
}
