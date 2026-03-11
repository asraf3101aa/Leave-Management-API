using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Persistence;
using System.Linq;

namespace LeaveManagement.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly MasterDbContext _masterContext;
    private readonly Guid? _tenantId;
    private string? _connectionString;

    public TenantService(MasterDbContext masterContext, IHttpContextAccessor httpContextAccessor)
    {
        _masterContext = masterContext;
        var tenantHeader = httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
        
        if (Guid.TryParse(tenantHeader, out var tid))
        {
            _tenantId = tid;
            var tenant = _masterContext.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.Id == _tenantId);
            if (tenant != null)
            {
                _connectionString = tenant.ConnectionString;
            }
        }
    }

    public Guid? GetTenantId() => _tenantId;
    public string? GetConnectionString() => _connectionString;
}
