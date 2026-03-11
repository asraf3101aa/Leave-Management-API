using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.ChangeLeaveRequestStatus;

public record SubmitLeaveRequestForReviewCommand(Guid LeaveRequestId, Guid ByUserId) : IRequest<bool>;
public class SubmitLeaveRequestForReviewCommandHandler(ILeaveService leaveService)
    : IRequestHandler<SubmitLeaveRequestForReviewCommand, bool>
{
    public async Task<bool> Handle(SubmitLeaveRequestForReviewCommand request, CancellationToken cancellationToken) =>
        await leaveService.SubmitForReview(request.LeaveRequestId, request.ByUserId, cancellationToken);
}

public record ApproveLeaveRequestCommand(Guid LeaveRequestId, Guid ByUserId) : IRequest<bool>;
public class ApproveLeaveRequestCommandHandler(ILeaveService leaveService)
    : IRequestHandler<ApproveLeaveRequestCommand, bool>
{
    public async Task<bool> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken) =>
        await leaveService.ApproveLeaveRequest(request.LeaveRequestId, request.ByUserId, cancellationToken);
}

public record RejectLeaveRequestCommand(Guid LeaveRequestId, Guid ByUserId) : IRequest<bool>;
public class RejectLeaveRequestCommandHandler(ILeaveService leaveService)
    : IRequestHandler<RejectLeaveRequestCommand, bool>
{
    public async Task<bool> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken) =>
        await leaveService.RejectLeaveRequest(request.LeaveRequestId, request.ByUserId, cancellationToken);
}
