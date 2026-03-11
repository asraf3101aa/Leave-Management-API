using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public class GetLeaveRequestListQueryHandler(ILeaveService leaveService)
    : IRequestHandler<GetLeaveRequestListQuery, PaginatedData<LeaveRequestListDto>>
{
    public async Task<PaginatedData<LeaveRequestListDto>> Handle(GetLeaveRequestListQuery request, CancellationToken cancellationToken) =>
        await leaveService.GetLeaveRequests(request.Page, request.PageSize, cancellationToken);
}
