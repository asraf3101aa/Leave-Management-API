using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagement.Api.Helpers;
using LeaveManagement.Application.Constants;
using LeaveManagement.Api.Authorization;
using LeaveManagement.Infrastructure.Services;
using LeaveManagement.Application.Features.LeaveTypes.Commands;
using LeaveManagement.Application.Features.LeaveTypes.Queries;
using LeaveManagement.Application.Responses;
using System.Threading.Tasks;

namespace LeaveManagement.Api.Controllers;

[Authorize]
public class LeaveTypesController(IMediator mediator, ResponseFactory responseFactory)
    : BaseController(mediator, responseFactory)
{

    [HttpGet]
    [AuthorizePermission(Permissions.LeaveTypes.View)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeInactive = false)
    {
        var result = await Mediator.Send(new GetLeaveTypesQuery(page, pageSize, includeInactive));
        return ApiResponse.Paginated(result.Items, result.Meta.TotalItems, result.Meta.CurrentPage, result.Meta.ItemsPerPage);
    }

    [HttpGet("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveTypes.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetLeaveTypeByIdQuery(id));
        return result is not null
            ? ApiResponse.Success(result)
            : ApiResponse.Error("NOT_FOUND", "Leave type not found", 404);
    }

    [HttpPost]
    [AuthorizePermission(Permissions.LeaveTypes.Create)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveTypeRequest request)
    {
        var id = await Mediator.Send(new CreateLeaveTypeCommand(
            request.Name,
            request.DefaultDays,
            CurrentUserId!.Value
        ));
        return ApiResponse.Created(id, "Leave type created successfully");
    }

    [HttpPut("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveTypes.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveTypeRequest request)
    {
        var success = await Mediator.Send(new UpdateLeaveTypeCommand(
            id,
            request.Name,
            request.DefaultDays,
            request.IsActive,
            CurrentUserId!.Value
        ));

        return success
            ? ApiResponse.Success<object?>(null, "Leave type updated successfully")
            : ApiResponse.Error("NOT_FOUND", "Leave type not found", 404);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveTypes.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await Mediator.Send(new DeleteLeaveTypeCommand(id, CurrentUserId!.Value));
        return success
            ? ApiResponse.Success<object?>(null, "Leave type deleted successfully")
            : ApiResponse.Error("NOT_FOUND", "Leave type not found", 404);
    }
}

public record CreateLeaveTypeRequest(string Name, int DefaultDays);
public record UpdateLeaveTypeRequest(string Name, int DefaultDays, bool IsActive);
