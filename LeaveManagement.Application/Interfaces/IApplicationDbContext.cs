using System.Threading;
using System.Threading.Tasks;
using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<LeaveType> LeaveTypes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
