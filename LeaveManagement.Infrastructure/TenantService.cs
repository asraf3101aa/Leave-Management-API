using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Infrastructure.Persistence;
namespace LeaveManagement.Infrastructure
{
    public class TenantService : ITenantService
    {
        private readonly SharedDbContext _sharedContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _tenantId;
        private string? _connectionString;

        public TenantService(SharedDbContext sharedContext, IHttpContextAccessor httpContextAccessor)
        {
            _sharedContext = sharedContext;
            _httpContextAccessor = httpContextAccessor;
            _tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
            
            {
                var tenant = _sharedContext.Tenants.FirstOrDefault(t => t.Id == _tenantId);
                if (tenant != null)
                {
                    _connectionString = tenant.ConnectionString;
                }
            }
        }

        public string? GetTenantId() => _tenantId;
        public string? GetConnectionString() => _connectionString;
    }
}
