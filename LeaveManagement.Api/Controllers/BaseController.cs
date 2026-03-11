using LeaveManagement.Api.Helpers;
using LeaveManagement.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(IMediator mediator, ResponseFactory responseFactory) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;
    protected readonly ResponseFactory ApiResponse = responseFactory;
    protected Guid? CurrentTenantId => Guid.TryParse(Request.Headers["X-Tenant-Id"].FirstOrDefault(), out var id) ? id : null;
    protected Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue("userId"), out var id) ? id : null;
}
