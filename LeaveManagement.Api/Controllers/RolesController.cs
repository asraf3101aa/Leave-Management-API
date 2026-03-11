using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Application.Constants;
using LeaveManagement.Api.Authorization;
using LeaveManagement.Infrastructure.Services;
using LeaveManagement.Api.Helpers;
using MediatR;
using LeaveManagement.Application.Features.Roles.Commands;

namespace LeaveManagement.Api.Controllers;

public class RolesController(IMediator mediator, ResponseFactory responseFactory) : BaseController(mediator, responseFactory)
{
    [HttpPost]
    [AuthorizePermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRoleModel model)
    {
        var targetTenantId = User.IsInRole(Roles.SuperAdmin) ? model.TenantId : CurrentTenantId;
        var result = await Mediator.Send(new CreateRoleCommand(model.RoleName, targetTenantId, CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Role created successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to create role", 400);
    }

    [HttpPost("set-default")]
    [AuthorizePermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> SetDefaultRole([FromBody] string roleName)
    {
        var result = await Mediator.Send(new SetDefaultRoleCommand(CurrentTenantId!.Value, roleName, CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Default role set successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to set default role", 400);
    }

    [HttpPost("{roleName}/permissions")]
    [AuthorizePermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> UpdatePermissions(string roleName, [FromBody] List<string> permissions)
    {
        var result = await Mediator.Send(new UpdateRolePermissionsCommand(roleName, CurrentTenantId, permissions, CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Role permissions updated successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to update role permissions", 400);
    }

    [HttpGet]
    [AuthorizePermission(Permissions.Roles.View)]
    public async Task<IActionResult> Get(Guid? tenantId)
    {
        var targetTenantId = User.IsInRole(Roles.SuperAdmin) ? tenantId : CurrentTenantId;
        var result = await Mediator.Send(new GetRolesQuery(targetTenantId));
        return ApiResponse.Success(result);
    }

    [HttpDelete("{roleName}")]
    [AuthorizePermission(Permissions.Roles.Delete)]
    public async Task<IActionResult> Delete(string roleName)
    {
        var result = await Mediator.Send(new DeleteRoleCommand(roleName, CurrentTenantId, CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Role deleted successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to delete role", 400);
    }
}
