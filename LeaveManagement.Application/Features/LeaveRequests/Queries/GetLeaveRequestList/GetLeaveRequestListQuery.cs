using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public record GetLeaveRequestListQuery : IRequest<List<LeaveRequestListDto>>;
