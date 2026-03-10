using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public class GetLeaveRequestListQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLeaveRequestListQuery, List<LeaveRequestListDto>>
{
    public async Task<List<LeaveRequestListDto>> Handle(GetLeaveRequestListQuery request, CancellationToken cancellationToken)
    {
        return await context.LeaveRequests
            .Select(lr => new LeaveRequestListDto(
                lr.Id,
                lr.RequestingEmployeeId,
                lr.StartDate,
                lr.EndDate,
                lr.LeaveTypeId,
                lr.Status,
                lr.Duration
            ))
            .ToListAsync(cancellationToken);
    }
}
