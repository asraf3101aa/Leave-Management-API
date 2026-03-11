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
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(p => p.Duration)
            .IsInEnum().WithMessage("Invalid {PropertyName}");

        RuleFor(p => p.CreatedBy)
            .NotEmpty().WithMessage("{PropertyName} is required");
    }
}
