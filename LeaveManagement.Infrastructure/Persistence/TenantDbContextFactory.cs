using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using LeaveManagement.Application.Interfaces;
using Moq;

namespace LeaveManagement.Infrastructure.Persistence;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TenantDbContext>();
        // Use a dummy connection for migration generation
        builder.UseNpgsql("Host=localhost;Database=MigrationDummy;Username=postgres;Password=postgres");

        var mockTenantService = new Mock<ITenantService>();
        mockTenantService.Setup(s => s.GetConnectionString()).Returns("Host=localhost;Database=MigrationDummy;Username=postgres;Password=postgres");

        return new TenantDbContext(builder.Options, mockTenantService.Object);
    }
}
