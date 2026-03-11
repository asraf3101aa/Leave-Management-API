using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry);

public interface IAuthService
{
    Task<AuthResponse?> Login(string email, string password);
    Task<bool> Register(string email, string password, string firstName, string lastName, string? roleName = null, Guid? tenantId = null, Guid? changedBy = null);
    Task<bool> AssignRoleToUserAsync(Guid userId, string roleName, Guid? tenantId, Guid changedBy);
    Task<bool> CreateRole(string roleName, Guid? tenantId, Guid changedBy);
    Task<IEnumerable<string>> GetRolesByTenant(Guid? tenantId);
    Task<bool> SoftDeleteUser(Guid userId);
    Task<bool> SoftDeleteRole(string roleName, Guid? tenantId, Guid changedBy);
    Task<bool> ToggleUserActive(Guid userId);
    Task<bool> CreateInvitation(string email, Guid tenantId, string roleName, Guid createdBy);
    Task<TenantInvitation?> GetInvitationByToken(string token);
    Task<bool> AcceptInvitation(string token, string password, string firstName, string lastName);
    Task<bool> SetDefaultRole(Guid tenantId, string roleName, Guid changedBy);
    Task<bool> UpsertRolePermissions(string roleName, Guid? tenantId, List<string> permissions, Guid changedBy);
    Task<List<string>> GetPermissionsByRole(string roleName, Guid? tenantId);
    Task<AuthResponse?> Refresh(string refreshToken);
    Task<List<Guid>> GetUserTenantIds(Guid userId);
    Task<bool> IsUserInTenant(Guid userId, Guid tenantId);
}
