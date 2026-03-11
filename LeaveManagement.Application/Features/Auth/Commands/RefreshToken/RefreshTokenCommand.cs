using MediatR;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse?>;

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, AuthResponse?>
{
    public async Task<AuthResponse?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authService.Refresh(request.RefreshToken);
    }
}
