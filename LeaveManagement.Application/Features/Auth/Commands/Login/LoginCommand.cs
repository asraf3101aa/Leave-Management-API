using MediatR;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse?>;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthResponse?>
{
    public async Task<AuthResponse?> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        await authService.Login(request.Email, request.Password);
}
