using MediatR;
using LeaveManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;

public class DeleteLeaveRequestCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<DeleteLeaveRequestCommand>
{
    public async Task Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var lr = await context.LeaveRequests
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (lr == null) throw new KeyNotFoundException("Leave request not found");

        context.LeaveRequests.Remove(lr);
        await context.SaveChangesAsync(cancellationToken);
    }
}
