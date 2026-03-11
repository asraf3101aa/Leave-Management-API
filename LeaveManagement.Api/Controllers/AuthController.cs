using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Application.Constants;
using LeaveManagement.Api.Authorization;
using LeaveManagement.Api.Helpers;
using MediatR;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Features.Auth.Commands;
using LeaveManagement.Application.Features.Auth.Queries.GetMe;
using LeaveManagement.Application.Features.Auth.Queries.GetUserTenants;

namespace LeaveManagement.Api.Controllers;

public class AuthController(IMediator mediator, ResponseFactory responseFactory) : BaseController(mediator, responseFactory)
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await Mediator.Send(new LoginCommand(model.Email, model.Password));
        if (result == null) return ApiResponse.Error("UNAUTHORIZED", "Invalid email or password", 401);

        SetAuthCookies(result);
        return ApiResponse.Success(new { Message = "Login successful" });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");
        return ApiResponse.Success<object?>(null, "Logout successful");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetMeQuery(CurrentUserId!.Value, CurrentTenantId, User));
        return result != null ? ApiResponse.Success(result) : ApiResponse.Error("NOT_FOUND", "User not found", 404);
    }

    [Authorize]
    [HttpGet("my-tenants")]
    public async Task<IActionResult> GetMyTenants([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await Mediator.Send(new GetUserTenantsQuery(CurrentUserId!.Value, page, pageSize));
        return ApiResponse.Paginated(result.Items, result.Meta.TotalItems, result.Meta.CurrentPage, result.Meta.ItemsPerPage);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (string.IsNullOrEmpty(refreshToken)) return ApiResponse.Error("UNAUTHORIZED", "Missing refresh token", 401);

        var result = await Mediator.Send(new RefreshTokenCommand(refreshToken));
        if (result == null) return ApiResponse.Error("UNAUTHORIZED", "Invalid or expired refresh token", 401);

        SetAuthCookies(result);
        return ApiResponse.Success(new { Message = "Token refreshed successfully" });
    }

    [HttpPost("invite-user")]
    [AuthorizePermission(Permissions.Users.Create)]
    public async Task<IActionResult> InviteUser([FromBody] InviteModel model)
    {
        var result = await Mediator.Send(new InviteUserCommand(model.Email, CurrentTenantId!.Value, model.Role ?? "Employee", CurrentUserId!.Value));
        return result ? ApiResponse.Success<object?>(null, "Invitation sent successfully") : ApiResponse.Error("BAD_REQUEST", "Failed to create invitation", 400);
    }

    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationModel model)
    {
        var result = await Mediator.Send(new AcceptInvitationCommand(model.Token, model.Password, model.FirstName, model.LastName));
        return result ? ApiResponse.Success<object?>(null, "Invitation accepted successfully") : ApiResponse.Error("BAD_REQUEST", "Invalid or expired invitation token.", 400);
    }

    [HttpPatch("users/{userId}/toggle-active")]
    [AuthorizePermission(Permissions.Users.Edit)]
    public async Task<IActionResult> ToggleUserActive(Guid userId)
    {
        var isSuperAdmin = User.IsInRole(Roles.SuperAdmin);
        var result = await Mediator.Send(new ToggleUserActiveCommand(userId, CurrentTenantId, isSuperAdmin));
        return result ? ApiResponse.Success<object?>(null, "User active status toggled successfully") : ApiResponse.Error("FORBIDDEN", "Unauthorized access or failure toggling user status", 403);
    }

    [HttpDelete("users/{userId}")]
    [AuthorizePermission(Permissions.Users.Delete)]
    public async Task<IActionResult> SoftDeleteUser(Guid userId)
    {
        var isSuperAdmin = User.IsInRole(Roles.SuperAdmin);
        var result = await Mediator.Send(new SoftDeleteUserCommand(userId, CurrentTenantId, isSuperAdmin));
        return result ? ApiResponse.Success<object?>(null, "User deleted successfully") : ApiResponse.Error("FORBIDDEN", "Unauthorized access or failure deleting user", 403);
    }

    private void SetAuthCookies(AuthResponse result)
    {
        var authCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = result.RefreshTokenExpiry
        };

        Response.Cookies.Append("AccessToken", result.AccessToken, authCookieOptions);
        Response.Cookies.Append("RefreshToken", result.RefreshToken, refreshCookieOptions);
    }
}

public record LoginModel(string Email, string Password);
public record InviteModel(string Email, string? Role);
public record AcceptInvitationModel(string Token, string Password, string FirstName, string LastName);
public record CreateRoleModel(string RoleName, Guid? TenantId);