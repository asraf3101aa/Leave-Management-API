using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Commands.UpdateLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;
using LeaveManagement.Application.Features.LeaveRequests.Commands.ChangeLeaveRequestStatus;
using LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;
using LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestDetails;
using LeaveManagement.Application.Constants;
using LeaveManagement.Api.Authorization;
using LeaveManagement.Api.Helpers;

namespace LeaveManagement.Api.Controllers;

[Authorize]
public class LeaveRequestsController(IMediator mediator, ResponseFactory responseFactory)
    : BaseController(mediator, responseFactory)
{
    // ─── CRUD ────────────────────────────────────────────────────────────────

    /// <summary>GET /api/leaverequests?page=1&pageSize=10</summary>
    [HttpGet]
    [AuthorizePermission(Permissions.LeaveRequests.View)]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetLeaveRequestListQuery(page, pageSize));
        return ApiResponse.Paginated(result.Items, result.Meta.TotalItems, result.Meta.CurrentPage, result.Meta.ItemsPerPage);
    }

    /// <summary>GET /api/leaverequests/{id}</summary>
    [HttpGet("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveRequests.View)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await Mediator.Send(new GetLeaveRequestDetailsQuery(id));
        return result != null
            ? ApiResponse.Success(result)
            : ApiResponse.Error("NOT_FOUND", "Leave request not found", 404);
    }

    /// <summary>POST /api/leaverequests — Create a new leave request (employee)</summary>
    [HttpPost]
    [AuthorizePermission(Permissions.LeaveRequests.Create)]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestCommand command)
    {
        var cmd = command with { CreatedBy = CurrentUserId!.Value };
        var id = await Mediator.Send(cmd);
        return ApiResponse.Created(id, "Leave request created successfully");
    }

    /// <summary>PUT /api/leaverequests/{id} — Edit a PENDING request (creator only)</summary>
    [HttpPut("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveRequests.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveRequestCommand command)
    {
        var cmd = command with { Id = id, UpdatedBy = CurrentUserId!.Value };
        await Mediator.Send(cmd);
        return ApiResponse.Success<object?>(null, "Leave request updated successfully");
    }

    /// <summary>DELETE /api/leaverequests/{id}</summary>
    [HttpDelete("{id:guid}")]
    [AuthorizePermission(Permissions.LeaveRequests.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteLeaveRequestCommand(id));
        return ApiResponse.Success<object?>(null, "Leave request deleted successfully");
    }

    // ─── Status Transitions ───────────────────────────────────────────────────

    /// <summary>PATCH /api/leaverequests/{id}/submit — Employee submits for review</summary>
    [HttpPatch("{id:guid}/submit")]
    [AuthorizePermission(Permissions.LeaveRequests.Edit)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await Mediator.Send(new SubmitLeaveRequestForReviewCommand(id, CurrentUserId!.Value));
        return result
            ? ApiResponse.Success<object?>(null, "Leave request submitted for review")
            : ApiResponse.Error("BAD_REQUEST", "Only pending requests can be submitted for review", 400);
    }

    /// <summary>PATCH /api/leaverequests/{id}/approve — Manager / HR approves an in-review request</summary>
    [HttpPatch("{id:guid}/approve")]
    [AuthorizePermission(Permissions.LeaveRequests.Approve)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await Mediator.Send(new ApproveLeaveRequestCommand(id, CurrentUserId!.Value));
        return result
            ? ApiResponse.Success<object?>(null, "Leave request approved")
            : ApiResponse.Error("BAD_REQUEST", "Only in-review requests can be approved", 400);
    }

    /// <summary>PATCH /api/leaverequests/{id}/reject — Manager / HR rejects a pending or in-review request</summary>
    [HttpPatch("{id:guid}/reject")]
    [AuthorizePermission(Permissions.LeaveRequests.Approve)]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await Mediator.Send(new RejectLeaveRequestCommand(id, CurrentUserId!.Value));
        return result
            ? ApiResponse.Success<object?>(null, "Leave request rejected")
            : ApiResponse.Error("BAD_REQUEST", "Request could not be rejected in its current state", 400);
    }
}
