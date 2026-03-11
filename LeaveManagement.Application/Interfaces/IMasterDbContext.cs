using Microsoft.EntityFrameworkCore;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Application.Interfaces;

public interface IMasterDbContext
{
    DbSet<Tenant> Tenants { get; set; }
    DbSet<TenantInvitation> TenantInvitations { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
