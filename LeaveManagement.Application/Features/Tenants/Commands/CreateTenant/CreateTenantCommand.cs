using MediatR;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(string Name, Guid CreatedBy) : IRequest<Guid>;

public class CreateTenantCommandHandler(IMasterDbContext context) : IRequestHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant 
        { 
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedBy = request.CreatedBy
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }
}
