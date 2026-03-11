using MediatR;
using System.Security.Claims;

namespace LeaveManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery(Guid UserId, Guid? TenantId, ClaimsPrincipal UserPrincipal) : IRequest<UserMeResponse?>;

public record UserMeResponse(
    Guid Id,
    string? Email,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions
);

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserMeResponse?>
{
    public Task<UserMeResponse?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = request.UserPrincipal;

        var response = new UserMeResponse(
            request.UserId,
            user.FindFirstValue(ClaimTypes.Email),
            user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value),
            user.Claims.Where(c => c.Type == "Permission").Select(c => c.Value)
        );

        return Task.FromResult<UserMeResponse?>(response);
    }
}
