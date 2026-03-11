using MediatR;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Auth.Queries.GetUserTenants;

public record UserTenantDto(Guid Id, string Name, bool IsActive);

public record GetUserTenantsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PaginatedData<UserTenantDto>>;

public class GetUserTenantsQueryHandler(IAuthService authService, IMasterDbContext context)
    : IRequestHandler<GetUserTenantsQuery, PaginatedData<UserTenantDto>>
{
    public async Task<PaginatedData<UserTenantDto>> Handle(GetUserTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenantIds = await authService.GetUserTenantIds(request.UserId);

        var totalCount = await context.Tenants
            .Where(t => tenantIds.Contains(t.Id))
            .CountAsync(cancellationToken);

        var items = await context.Tenants
            .Where(t => tenantIds.Contains(t.Id))
            .OrderBy(t => t.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new UserTenantDto(t.Id, t.Name, t.IsActive))
            .ToListAsync(cancellationToken);

        return new PaginatedData<UserTenantDto>
        {
            Items = items,
            Meta = new PaginationMeta
            {
                TotalItems = totalCount,
                ItemCount = items.Count,
                ItemsPerPage = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                CurrentPage = request.Page
            }
        };
    }
}
