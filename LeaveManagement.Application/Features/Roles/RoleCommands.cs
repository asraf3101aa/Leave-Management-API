using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Roles.Commands;

public record CreateRoleCommand(string RoleName, Guid? TenantId, Guid CreatedBy) : IRequest<bool>;
public class CreateRoleCommandHandler(IAuthService authService) : IRequestHandler<CreateRoleCommand, bool>
{
    public async Task<bool> Handle(CreateRoleCommand request, CancellationToken cancellationToken) =>
        await authService.CreateRole(request.RoleName, request.TenantId, request.CreatedBy);
}

public record SetDefaultRoleCommand(Guid TenantId, string RoleName, Guid UpdatedBy) : IRequest<bool>;
public class SetDefaultRoleCommandHandler(IAuthService authService) : IRequestHandler<SetDefaultRoleCommand, bool>
{
    public async Task<bool> Handle(SetDefaultRoleCommand request, CancellationToken cancellationToken) =>
        await authService.SetDefaultRole(request.TenantId, request.RoleName, request.UpdatedBy);
}

public record UpdateRolePermissionsCommand(string RoleName, Guid? TenantId, List<string> Permissions, Guid UpdatedBy) : IRequest<bool>;
public class UpdateRolePermissionsCommandHandler(IAuthService authService) : IRequestHandler<UpdateRolePermissionsCommand, bool>
{
    public async Task<bool> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken) =>
        await authService.UpsertRolePermissions(request.RoleName, request.TenantId, request.Permissions, request.UpdatedBy);
}

public record GetRolesQuery(Guid? TenantId) : IRequest<IEnumerable<string>>;
public class GetRolesQueryHandler(IAuthService authService) : IRequestHandler<GetRolesQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        await authService.GetRolesByTenant(request.TenantId);
}

public record DeleteRoleCommand(string RoleName, Guid? TenantId, Guid DeletedBy) : IRequest<bool>;
public class DeleteRoleCommandHandler(IAuthService authService) : IRequestHandler<DeleteRoleCommand, bool>
{
    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken) =>
        await authService.SoftDeleteRole(request.RoleName, request.TenantId, request.DeletedBy);
}
