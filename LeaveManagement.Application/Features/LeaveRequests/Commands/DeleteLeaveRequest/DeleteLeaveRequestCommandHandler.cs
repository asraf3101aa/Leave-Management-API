using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;

public class DeleteLeaveRequestCommandHandler(ILeaveService leaveService)
    : IRequestHandler<DeleteLeaveRequestCommand>
{
    public async Task Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken) =>
        await leaveService.DeleteLeaveRequest(request.Id, cancellationToken);
}
