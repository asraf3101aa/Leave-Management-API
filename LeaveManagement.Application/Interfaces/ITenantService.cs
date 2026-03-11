using System;
namespace LeaveManagement.Application.Interfaces
{
    public interface ITenantService
    {
        Guid? GetTenantId();
        string? GetConnectionString();
    }
}
