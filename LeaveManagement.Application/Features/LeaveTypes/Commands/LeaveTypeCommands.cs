using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveTypes.Commands;

public record CreateLeaveTypeCommand(string Name, int DefaultDays, Guid CreatedBy) : IRequest<Guid>;

public class CreateLeaveTypeCommandHandler(ILeaveService leaveService)
    : IRequestHandler<CreateLeaveTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken) =>
        await leaveService.CreateLeaveType(request.Name, request.DefaultDays, request.CreatedBy, cancellationToken);
}

public record UpdateLeaveTypeCommand(Guid Id, string Name, int DefaultDays, bool IsActive, Guid UpdatedBy) : IRequest<bool>;

public class UpdateLeaveTypeCommandHandler(ILeaveService leaveService)
    : IRequestHandler<UpdateLeaveTypeCommand, bool>
{
    public async Task<bool> Handle(UpdateLeaveTypeCommand request, CancellationToken cancellationToken) =>
        await leaveService.UpdateLeaveType(request.Id, request.Name, request.DefaultDays, request.IsActive, request.UpdatedBy, cancellationToken);
}

public record DeleteLeaveTypeCommand(Guid Id, Guid DeletedBy) : IRequest<bool>;

public class DeleteLeaveTypeCommandHandler(ILeaveService leaveService)
    : IRequestHandler<DeleteLeaveTypeCommand, bool>
{
    public async Task<bool> Handle(DeleteLeaveTypeCommand request, CancellationToken cancellationToken) =>
        await leaveService.DeleteLeaveType(request.Id, request.DeletedBy, cancellationToken);
}
