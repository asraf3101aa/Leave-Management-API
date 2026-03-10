using System.Linq;
using Microsoft.AspNetCore.Http;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Persistence;

namespace LeaveManagement.Infrastructure.Services;

public class TenantService(MasterDbContext masterContext, IHttpContextAccessor httpContextAccessor) : ITenantService
{
    private readonly string? _tenantId = httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
    private string? _connectionString;

    public string? GetTenantId() => _tenantId;

    public string? GetConnectionString()
    {
        if (string.IsNullOrEmpty(_tenantId)) return null;
        if (_connectionString != null) return _connectionString;

        var tenant = masterContext.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.Id == _tenantId);
        _connectionString = tenant?.ConnectionString;
        return _connectionString;
    }
}
