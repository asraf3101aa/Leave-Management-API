using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Domain.Models;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Infrastructure.Persistence;

public class MasterDbContext(DbContextOptions<MasterDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        string,
        IdentityUserClaim<string>,
        TenantUserRole,
        IdentityUserLogin<string>,
        ApplicationRoleClaim,
        IdentityUserToken<string>>(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantHistory> TenantHistories { get; set; } = null!;
    public DbSet<TenantInvitation> TenantInvitations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tenant Configuration
        builder.Entity<Tenant>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        });

        builder.Entity<TenantInvitation>(b =>
        {
            b.HasIndex(i => i.Token).IsUnique();
            b.HasIndex(i => i.Email);
        });

        // Identity Configuration (Modified for Master DB / RBAC)
        builder.Entity<TenantUserRole>(b =>
        {
            b.ToTable("AspNetUserRoles");
            b.HasKey(ur => new { ur.UserId, ur.RoleId, ur.TenantId });
            b.HasQueryFilter(ur => !ur.IsDeleted);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.HasIndex(r => new { r.Name, r.TenantId })
                .IsUnique();
            b.HasQueryFilter(r => !r.IsDeleted);
        });

        builder.Entity<ApplicationRoleClaim>(b =>
        {
            b.ToTable("AspNetRoleClaims");
            b.HasQueryFilter(rc => !rc.IsDeleted);
        });

        builder.Entity<ApplicationUser>(b =>
        {
            b.HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        });
    }
}
