using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Features.LeaveTypes.Queries;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Responses;
using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace LeaveManagement.Infrastructure.Services;

public class LeaveService(IApplicationDbContext context) : ILeaveService
{
    //   Leave Requests ───────────────────────────────────────────────────────

    public async Task<Guid> CreateLeaveRequest(DateTime startDate, DateTime endDate, Guid leaveTypeId, string comments, LeaveDuration duration, Guid createdBy, CancellationToken ct = default)
    {
        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            LeaveTypeId = leaveTypeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            RequestComments = comments,
            Status = LeaveStatus.Pending,
            Duration = duration
        };

        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync(ct);
        return leaveRequest.Id;
    }

    public async Task UpdateLeaveRequest(Guid id, DateTime startDate, DateTime endDate, Guid leaveTypeId, string comments, LeaveDuration duration, Guid updatedBy, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave request not found");

        if (lr.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending leave requests can be edited.");

        lr.StartDate = startDate;
        lr.EndDate = endDate;
        lr.LeaveTypeId = leaveTypeId;
        lr.RequestComments = comments;
        lr.Duration = duration;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteLeaveRequest(Guid id, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave request not found");

        context.LeaveRequests.Remove(lr);
        await context.SaveChangesAsync(ct);
    }

    public async Task<LeaveRequestDto?> GetLeaveRequest(Guid id, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (lr == null) return null;

        return new LeaveRequestDto(
            lr.Id, lr.StartDate, lr.EndDate, lr.LeaveTypeId, lr.CreatedAt, lr.CreatedBy,
            lr.RequestComments, lr.Status, lr.Duration, lr.ApprovedBy, lr.ReviewedBy,
            lr.ApprovedAt, lr.ReviewedAt
        );
    }

    public async Task<PaginatedData<LeaveRequestListDto>> GetLeaveRequests(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = context.LeaveRequests.OrderByDescending(q => q.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(lr => new LeaveRequestListDto(lr.Id, lr.StartDate, lr.EndDate, lr.LeaveTypeId, lr.Status, lr.Duration, lr.CreatedBy))
            .ToListAsync(ct);

        return new PaginatedData<LeaveRequestListDto>
        {
            Items = items,
            Meta = new PaginationMeta
            {
                TotalItems = total,
                ItemCount = items.Count,
                ItemsPerPage = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                CurrentPage = page
            }
        };
    }

    // ─── Status Transitions ───────────────────────────────────────────────────

    public async Task<bool> SubmitForReview(Guid id, Guid byUserId, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (lr == null || lr.Status != LeaveStatus.Pending) return false;

        lr.Status = LeaveStatus.InReview;
        lr.ReviewedBy = byUserId;
        lr.ReviewedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ApproveLeaveRequest(Guid id, Guid byUserId, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (lr == null || lr.Status != LeaveStatus.InReview) return false;

        lr.Status = LeaveStatus.Approved;
        lr.ApprovedBy = byUserId;
        lr.ApprovedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectLeaveRequest(Guid id, Guid byUserId, CancellationToken ct = default)
    {
        var lr = await context.LeaveRequests.FirstOrDefaultAsync(q => q.Id == id, ct);
        if (lr == null || (lr.Status != LeaveStatus.InReview && lr.Status != LeaveStatus.Pending)) return false;

        lr.Status = LeaveStatus.Rejected;
        lr.ReviewedBy = byUserId;
        lr.ReviewedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return true;
    }

    // ─── Leave Types ──────────────────────────────────────────────────────────

    public async Task<Guid> CreateLeaveType(string name, int defaultDays, Guid createdBy, CancellationToken ct = default)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = name,
            DefaultDays = defaultDays,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync(ct);
        return leaveType.Id;
    }

    public async Task<bool> UpdateLeaveType(Guid id, string name, int defaultDays, bool isActive, Guid updatedBy, CancellationToken ct = default)
    {
        var leaveType = await context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == id && !lt.IsDeleted, ct);
        if (leaveType == null) return false;

        leaveType.Name = name;
        leaveType.DefaultDays = defaultDays;
        leaveType.IsActive = isActive;
        leaveType.UpdatedBy = updatedBy;
        leaveType.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteLeaveType(Guid id, Guid deletedBy, CancellationToken ct = default)
    {
        var leaveType = await context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == id && !lt.IsDeleted, ct);
        if (leaveType == null) return false;

        leaveType.IsDeleted = true;
        leaveType.DeletedAt = DateTime.UtcNow;
        leaveType.UpdatedBy = deletedBy;
        leaveType.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<LeaveTypeDto?> GetLeaveType(Guid id, CancellationToken ct = default)
    {
        return await context.LeaveTypes
            .Where(lt => lt.Id == id && !lt.IsDeleted)
            .Select(lt => new LeaveTypeDto(lt.Id, lt.Name, lt.DefaultDays, lt.IsActive))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedData<LeaveTypeDto>> GetLeaveTypes(int page = 1, int pageSize = 10, bool includeInactive = false, CancellationToken ct = default)
    {
        var query = context.LeaveTypes.Where(lt => !lt.IsDeleted);
        if (!includeInactive)
            query = query.Where(lt => lt.IsActive);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(lt => lt.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(lt => new LeaveTypeDto(lt.Id, lt.Name, lt.DefaultDays, lt.IsActive))
            .ToListAsync(ct);

        return new PaginatedData<LeaveTypeDto>
        {
            Items = items,
            Meta = new PaginationMeta
            {
                TotalItems = total,
                ItemCount = items.Count,
                ItemsPerPage = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                CurrentPage = page
            }
        };
    }
}

