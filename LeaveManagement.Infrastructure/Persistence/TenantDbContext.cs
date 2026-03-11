using Microsoft.EntityFrameworkCore;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Infrastructure.Persistence;

public class TenantDbContext(DbContextOptions<TenantDbContext> options, ITenantService tenantService)
    : DbContext(options), IApplicationDbContext
{
    private readonly string _connectionString = tenantService.GetConnectionString() ?? string.Empty;

    public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
    public DbSet<LeaveType> LeaveTypes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
