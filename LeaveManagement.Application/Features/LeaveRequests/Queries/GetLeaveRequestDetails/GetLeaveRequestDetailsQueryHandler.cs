using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestDetails;

public class GetLeaveRequestDetailsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLeaveRequestDetailsQuery, LeaveRequestDto?>
{
    public async Task<LeaveRequestDto?> Handle(GetLeaveRequestDetailsQuery request, CancellationToken cancellationToken)
    {
        var lr = await context.LeaveRequests
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (lr == null) return null;

        return new LeaveRequestDto(
            lr.Id,
            lr.RequestingEmployeeId,
            lr.StartDate,
            lr.EndDate,
            lr.LeaveTypeId,
            lr.DateRequested,
            lr.RequestComments,
            lr.Status,
            lr.Cancelled,
            lr.Duration,
            lr.ApprovedBy,
            lr.UpdatedBy,
            lr.UpdatedAt
        );
    }
}
