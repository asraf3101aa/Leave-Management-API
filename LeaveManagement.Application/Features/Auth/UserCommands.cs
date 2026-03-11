using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Auth.Commands;

public record InviteUserCommand(string Email, Guid TenantId, string Role, Guid InvitedBy) : IRequest<bool>;
public class InviteUserCommandHandler(IAuthService authService) : IRequestHandler<InviteUserCommand, bool>
{
    public async Task<bool> Handle(InviteUserCommand request, CancellationToken cancellationToken) =>
        await authService.CreateInvitation(request.Email, request.TenantId, request.Role, request.InvitedBy);
}

public record AcceptInvitationCommand(string Token, string Password, string FirstName, string LastName) : IRequest<bool>;
public class AcceptInvitationCommandHandler(IAuthService authService) : IRequestHandler<AcceptInvitationCommand, bool>
{
    public async Task<bool> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken) =>
        await authService.AcceptInvitation(request.Token, request.Password, request.FirstName, request.LastName);
}

public record ToggleUserActiveCommand(Guid UserId, Guid? CurrentTenantId, bool IsSuperAdmin) : IRequest<bool>;
public class ToggleUserActiveCommandHandler(IAuthService authService) : IRequestHandler<ToggleUserActiveCommand, bool>
{
    public async Task<bool> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsSuperAdmin && request.CurrentTenantId.HasValue && request.CurrentTenantId != Guid.Empty)
        {
            var isUserInTenant = await authService.IsUserInTenant(request.UserId, request.CurrentTenantId.Value);
            if (!isUserInTenant) return false;
        }
        return await authService.ToggleUserActive(request.UserId);
    }
}

public record SoftDeleteUserCommand(Guid UserId, Guid? CurrentTenantId, bool IsSuperAdmin) : IRequest<bool>;
public class SoftDeleteUserCommandHandler(IAuthService authService) : IRequestHandler<SoftDeleteUserCommand, bool>
{
    public async Task<bool> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsSuperAdmin && request.CurrentTenantId.HasValue && request.CurrentTenantId != Guid.Empty)
        {
            var isUserInTenant = await authService.IsUserInTenant(request.UserId, request.CurrentTenantId.Value);
            if (!isUserInTenant) return false;
        }
        return await authService.SoftDeleteUser(request.UserId);
    }
}
