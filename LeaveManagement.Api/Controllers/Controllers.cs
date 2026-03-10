using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;
using LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestDetails;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Infrastructure.Persistence;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Models;
using LeaveManagement.Infrastructure.Services;
using LeaveManagement.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;
    protected string? CurrentTenantId => Request.Headers["X-Tenant-Id"].ToString() ?? User.FindFirstValue("tenant_id");
    protected string? CurrentUserId => User.FindFirstValue("uid");
}

[Authorize]
public class LeaveRequestsController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    [AuthorizePermission(Permissions.LeaveRequests.View)]
    public async Task<ActionResult<List<LeaveRequestListDto>>> Get() => Ok(await Mediator.Send(new GetLeaveRequestListQuery()));

    [HttpGet("{id}")]
    [AuthorizePermission(Permissions.LeaveRequests.View)]
    public async Task<ActionResult<LeaveRequestDto>> Get(Guid id)
    {
        var result = await Mediator.Send(new GetLeaveRequestDetailsQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [AuthorizePermission(Permissions.LeaveRequests.Create)]
    public async Task<ActionResult<Guid>> Create(CreateLeaveRequestCommand command) => Ok(await Mediator.Send(command));

    [HttpPut]
    [AuthorizePermission(Permissions.LeaveRequests.Edit)]
    public async Task<ActionResult> Update(UpdateLeaveRequestCommand command)
    {
        var updatedCommand = command with { UpdatedBy = CurrentUserId ?? "Unknown" };
        await Mediator.Send(updatedCommand);
        return NoContent();
    }

    [HttpPatch("{id}/approve")]
    [AuthorizePermission(Permissions.LeaveRequests.Approve)]
    public async Task<ActionResult> Approve(Guid id)
    {
        await Mediator.Send(new UpdateLeaveRequestCommand(id, LeaveStatus.Approved, LeaveDuration.FullDay, CurrentUserId!));
        return NoContent();
    }

    [HttpDelete("{id}")]
    [AuthorizePermission(Permissions.LeaveRequests.Delete)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteLeaveRequestCommand(id));
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class TenantsController(MasterDbContext context) : BaseController(null!)
{
    [HttpGet]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<IEnumerable<Tenant>>> Get(bool includeInactive = false)
    {
        IQueryable<Tenant> query = context.Tenants;
        if (includeInactive) query = query.IgnoreQueryFilters().Where(t => !t.IsDeleted);
        return await query.ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<string>> Create(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.CreatedBy = CurrentUserId ?? "System";
        tenant.IsActive = true;

        context.Tenants.Add(tenant);
        context.TenantHistories.Add(new TenantHistory
        {
            TenantId = tenant.Id,
            Action = "Created",
            Details = $"Created tenant {tenant.Name}",
            ChangedBy = CurrentUserId ?? "System"
        });

        await context.SaveChangesAsync();
        return Ok(tenant.Id);
    }

    [HttpPost("{id}/invite-admin")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> InviteAdmin(string id, [FromBody] InviteModel model, [FromServices] IAuthService authService)
    {
        var result = await authService.CreateInvitation(model.Email, id, Roles.Admin, CurrentUserId!);
        return result ? Ok() : BadRequest();
    }
}

[ApiController]
[Route("api/[controller]")]
public class RolesController(IAuthService authService, MasterDbContext masterContext) : BaseController(null!)
{
    [HttpPost]
    [AuthorizePermission(Permissions.Roles.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRoleModel model)
    {
        var targetTenantId = User.IsInRole(Roles.SuperAdmin) ? model.TenantId : CurrentTenantId;
        var result = await authService.CreateRole(model.RoleName, targetTenantId, CurrentUserId ?? "System");
        return result ? Ok() : BadRequest();
    }

    [HttpPost("set-default")]
    [AuthorizePermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> SetDefaultRole([FromBody] string roleName)
    {
        var result = await authService.SetDefaultRole(CurrentTenantId!, roleName, CurrentUserId ?? "System");
        return result ? Ok() : BadRequest();
    }

    [HttpPost("{roleName}/permissions")]
    [AuthorizePermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> UpdatePermissions(string roleName, [FromBody] List<string> permissions)
    {
        var result = await authService.UpsertRolePermissions(roleName, CurrentTenantId, permissions, CurrentUserId!);
        return result ? Ok() : BadRequest();
    }

    [HttpGet]
    [AuthorizePermission(Permissions.Roles.View)]
    public async Task<ActionResult<IEnumerable<string>>> Get(string? tenantId)
    {
        var targetTenantId = User.IsInRole(Roles.SuperAdmin) ? tenantId : CurrentTenantId;
        return Ok(await authService.GetRolesByTenant(targetTenantId));
    }

    [HttpDelete("{roleName}")]
    [AuthorizePermission(Permissions.Roles.Delete)]
    public async Task<IActionResult> Delete(string roleName)
    {
        var result = await authService.SoftDeleteRole(roleName, CurrentTenantId, CurrentUserId ?? "System");
        return result ? Ok() : BadRequest();
    }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, MasterDbContext masterContext) : BaseController(null!)
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var token = await authService.Login(model.Email, model.Password, model.TenantId);
        return string.IsNullOrEmpty(token) ? Unauthorized() : Ok(new { Token = token });
    }

    [HttpPost("invite-user")]
    [AuthorizePermission(Permissions.Users.Create)]
    public async Task<IActionResult> InviteUser([FromBody] InviteModel model)
    {
        var result = await authService.CreateInvitation(model.Email, CurrentTenantId!, model.Role ?? "Employee", CurrentUserId!);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationModel model)
    {
        var result = await authService.AcceptInvitation(model.Token, model.Password, model.FirstName, model.LastName);
        return result ? Ok() : BadRequest("Invalid or expired invitation token.");
    }

    [HttpPatch("users/{userId}/toggle-active")]
    [AuthorizePermission(Permissions.Users.Edit)]
    public async Task<IActionResult> ToggleUserActive(string userId)
    {
        if (User.IsInRole(Roles.Admin))
        {
            var isUserInTenant = await masterContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.TenantId == CurrentTenantId);
            if (!isUserInTenant) return Unauthorized();
        }
        var result = await authService.ToggleUserActive(userId);
        return result ? Ok() : BadRequest();
    }

    [HttpDelete("users/{userId}")]
    [AuthorizePermission(Permissions.Users.Delete)]
    public async Task<IActionResult> SoftDeleteUser(string userId)
    {
        if (User.IsInRole(Roles.Admin))
        {
            var isUserInTenant = await masterContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.TenantId == CurrentTenantId);
            if (!isUserInTenant) return Unauthorized();
        }
        var result = await authService.SoftDeleteUser(userId);
        return result ? NoContent() : BadRequest();
    }
}

public record LoginModel(string Email, string Password, string? TenantId);
public record InviteModel(string Email, string? Role);
public record AcceptInvitationModel(string Token, string Password, string FirstName, string LastName);
public record CreateRoleModel(string RoleName, string? TenantId);
