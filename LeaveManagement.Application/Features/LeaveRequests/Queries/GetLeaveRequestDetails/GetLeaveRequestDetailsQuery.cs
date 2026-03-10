using MediatR;
using LeaveManagement.Application.DTOs.LeaveRequest;

namespace LeaveManagement.Application.Features.LeaveRequests.Queries.GetLeaveRequestDetails;

public record GetLeaveRequestDetailsQuery(Guid Id) : IRequest<LeaveRequestDto?>;
