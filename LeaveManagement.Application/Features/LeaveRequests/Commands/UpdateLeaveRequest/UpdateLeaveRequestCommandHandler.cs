using MediatR;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;

public class UpdateLeaveRequestCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateLeaveRequestCommand>
{
    public async Task Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var lr = await context.LeaveRequests
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (lr == null) throw new KeyNotFoundException("Leave request not found");

        lr.StartDate = request.StartDate;
        lr.EndDate = request.EndDate;
        lr.LeaveTypeId = request.LeaveTypeId;
        lr.RequestComments = request.RequestComments;
        lr.Status = request.Status;
        lr.Cancelled = request.Cancelled;
        lr.Duration = request.Duration;

        lr.UpdatedBy = request.UpdatedBy;
        lr.UpdatedAt = DateTime.UtcNow;

        // If approval status changed, we can track who approved it.
        if (request.Status == LeaveStatus.Approved)
        {
            lr.ApprovedBy = request.UpdatedBy;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
