using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Infrastructure.Identity;
using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Infrastructure.Persistence;

public class MasterDbContext(DbContextOptions<MasterDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        TenantUserRole,
        IdentityUserLogin<Guid>,
        ApplicationRoleClaim,
        IdentityUserToken<Guid>>(options), IMasterDbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantInvitation> TenantInvitations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tenant Configuration
        builder.Entity<Tenant>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasQueryFilter(e => !e.IsDeleted && e.IsActive);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .IsRequired();

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(t => t.UpdatedBy);
        });



        builder.Entity<TenantInvitation>(b =>
        {
            b.HasIndex(i => i.Token).IsUnique();
            b.HasIndex(i => i.Email);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(i => i.CreatedBy)
                .IsRequired();
        });

        // Identity Configuration (Modified for Master DB / RBAC)
        builder.Entity<TenantUserRole>(b =>
        {
            b.ToTable("AspNetUserRoles");
            b.HasKey(ur => new { ur.UserId, ur.RoleId, ur.TenantId });
            b.HasQueryFilter(ur => !ur.IsDeleted);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ur => ur.CreatedBy);
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.HasIndex(r => new { r.Name, r.TenantId })
                .IsUnique();
            b.HasQueryFilter(r => !r.IsDeleted);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.CreatedBy);

            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UpdatedBy);
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
