using Microsoft.EntityFrameworkCore;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public class TenantDbContext(DbContextOptions<TenantDbContext> options, ITenantService tenantService)
    : DbContext(options), IApplicationDbContext
{
    private readonly string? _connectionString = tenantService.GetConnectionString();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }


    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
