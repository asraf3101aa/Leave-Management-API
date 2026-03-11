using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Application.Constants;
using LeaveManagement.Api.Authorization;
using LeaveManagement.Api.Helpers;
using MediatR;
using LeaveManagement.Application.Features.Tenants.Queries.GetTenants;
using LeaveManagement.Application.Features.Tenants.Commands.CreateTenant;
using LeaveManagement.Application.Features.Auth.Commands;

namespace LeaveManagement.Api.Controllers;

public class TenantsController(IMediator mediator, ResponseFactory responseFactory) : BaseController(mediator, responseFactory)
{
    [HttpGet]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> Get(bool includeInactive = false)
    {
        var tenants = await Mediator.Send(new GetTenantsQuery(includeInactive));
        return ApiResponse.Success(tenants);
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateTenantModel model)
    {
        var id = await Mediator.Send(new CreateTenantCommand(model.Name, CurrentUserId!.Value));
        return ApiResponse.Created(id, "Tenant created successfully");
    }

    [HttpPost("{id}/invite-admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> InviteAdmin(Guid id, [FromBody] InviteModel model)
    {
        var result = await Mediator.Send(new InviteUserCommand(model.Email, id, Roles.Admin, CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Invitation sent successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to create invitation", 400);
    }
}

public record CreateTenantModel(string Name);
