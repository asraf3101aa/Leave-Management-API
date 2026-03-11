using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestDetails;

public class GetLeaveRequestDetailsQueryHandler(ILeaveService leaveService)
    : IRequestHandler<GetLeaveRequestDetailsQuery, LeaveRequestDto?>
{
    public async Task<LeaveRequestDto?> Handle(GetLeaveRequestDetailsQuery request, CancellationToken cancellationToken) =>
        await leaveService.GetLeaveRequest(request.Id, cancellationToken);
}
