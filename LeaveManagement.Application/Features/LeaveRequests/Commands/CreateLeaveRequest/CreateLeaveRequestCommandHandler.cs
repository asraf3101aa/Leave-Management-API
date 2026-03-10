using MediatR;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public class CreateLeaveRequestCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateLeaveRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            RequestingEmployeeId = request.RequestingEmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            LeaveTypeId = request.LeaveTypeId,
            DateRequested = DateTime.UtcNow,
            RequestComments = request.RequestComments,
            Approved = null,
            Cancelled = false,
            Duration = request.Duration
        };

        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync(cancellationToken);
        return leaveRequest.Id;
    }
}
