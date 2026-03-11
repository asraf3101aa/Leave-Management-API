using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;
using LeaveManagement.Application.Responses;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public record GetLeaveRequestListQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedData<LeaveRequestListDto>>;
