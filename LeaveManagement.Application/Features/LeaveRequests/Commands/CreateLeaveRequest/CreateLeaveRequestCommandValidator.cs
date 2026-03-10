using FluentValidation;
using LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

namespace LeaveManagement.Application.Features.LeaveRequests.Commands.CreateLeaveRequest;

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(p => p.StartDate)
            .LessThan(p => p.EndDate).WithMessage("{PropertyName} must be before {ComparisonValue}");

        RuleFor(p => p.LeaveTypeId)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero");

        RuleFor(p => p.Duration)
            .IsInEnum().WithMessage("Invalid {PropertyName}");

        RuleFor(p => p.RequestingEmployeeId)
            .NotEmpty().WithMessage("{PropertyName} is required");
    }
}
