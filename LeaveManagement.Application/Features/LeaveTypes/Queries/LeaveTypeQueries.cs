using MediatR;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveManagement.Application.Features.LeaveTypes.Queries;

public record LeaveTypeDto(Guid Id, string Name, int DefaultDays, bool IsActive);

public record GetLeaveTypesQuery(int Page = 1, int PageSize = 10, bool IncludeInactive = false) : IRequest<PaginatedData<LeaveTypeDto>>;

public class GetLeaveTypesQueryHandler(ILeaveService leaveService)
    : IRequestHandler<GetLeaveTypesQuery, PaginatedData<LeaveTypeDto>>
{
    public async Task<PaginatedData<LeaveTypeDto>> Handle(GetLeaveTypesQuery request, CancellationToken cancellationToken) =>
        await leaveService.GetLeaveTypes(request.Page, request.PageSize, request.IncludeInactive, cancellationToken);
}

public record GetLeaveTypeByIdQuery(Guid Id) : IRequest<LeaveTypeDto?>;

public class GetLeaveTypeByIdQueryHandler(ILeaveService leaveService)
    : IRequestHandler<GetLeaveTypeByIdQuery, LeaveTypeDto?>
{
    public async Task<LeaveTypeDto?> Handle(GetLeaveTypeByIdQuery request, CancellationToken cancellationToken) =>
        await leaveService.GetLeaveType(request.Id, cancellationToken);
}
