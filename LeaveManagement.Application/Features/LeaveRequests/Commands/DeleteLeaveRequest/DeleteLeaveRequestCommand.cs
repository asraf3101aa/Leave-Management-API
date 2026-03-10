using MediatR;
namespace LeaveManagement.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;

public record DeleteLeaveRequestCommand(Guid Id) : IRequest;
