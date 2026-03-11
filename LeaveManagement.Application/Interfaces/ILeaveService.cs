using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Features.LeaveTypes.Queries;
using LeaveManagement.Application.Responses;
using LeaveManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Interfaces;

public interface ILeaveService
{
    // Leave Requests — CRUD
    Task<Guid> CreateLeaveRequest(DateTime startDate, DateTime endDate, Guid leaveTypeId, string comments, LeaveDuration duration, Guid createdBy, CancellationToken ct = default);
    Task UpdateLeaveRequest(Guid id, DateTime startDate, DateTime endDate, Guid leaveTypeId, string comments, LeaveDuration duration, Guid updatedBy, CancellationToken ct = default);
    Task DeleteLeaveRequest(Guid id, CancellationToken ct = default);
    Task<LeaveRequestDto?> GetLeaveRequest(Guid id, CancellationToken ct = default);
    Task<PaginatedData<LeaveRequestListDto>> GetLeaveRequests(int page = 1, int pageSize = 10, CancellationToken ct = default);

    // Leave Requests — Status Transitions
    Task<bool> SubmitForReview(Guid id, Guid byUserId, CancellationToken ct = default);
    Task<bool> ApproveLeaveRequest(Guid id, Guid byUserId, CancellationToken ct = default);
    Task<bool> RejectLeaveRequest(Guid id, Guid byUserId, CancellationToken ct = default);

    // Leave Types
    Task<Guid> CreateLeaveType(string name, int defaultDays, Guid createdBy, CancellationToken ct = default);
    Task<bool> UpdateLeaveType(Guid id, string name, int defaultDays, bool isActive, Guid updatedBy, CancellationToken ct = default);
    Task<bool> DeleteLeaveType(Guid id, Guid deletedBy, CancellationToken ct = default);
    Task<LeaveTypeDto?> GetLeaveType(Guid id, CancellationToken ct = default);
    Task<PaginatedData<LeaveTypeDto>> GetLeaveTypes(int page = 1, int pageSize = 10, bool includeInactive = false, CancellationToken ct = default);
}

