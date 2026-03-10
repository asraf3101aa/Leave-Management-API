using System;
namespace LeaveManagement.Application.Interfaces
{
    public interface ITenantService
    {
        string? GetTenantId();
        string? GetConnectionString();
    }
}
