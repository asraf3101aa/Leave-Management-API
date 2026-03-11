using MediatR;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Tenants.Queries.GetTenants;

public record GetTenantsQuery(bool IncludeInactive = false) : IRequest<List<Tenant>>;

public class GetTenantsQueryHandler(IMasterDbContext context) : IRequestHandler<GetTenantsQuery, List<Tenant>>
{
    public async Task<List<Tenant>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Tenant> query = context.Tenants;
        if (request.IncludeInactive) query = query.IgnoreQueryFilters().Where(t => !t.IsDeleted);
        return await query.ToListAsync(cancellationToken);
    }
}
