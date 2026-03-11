using FluentValidation;
using LeaveManagement.Application.Features.LeaveTypes.Commands;

namespace LeaveManagement.Application.Features.LeaveTypes.Validators;

public class CreateLeaveTypeCommandValidator : AbstractValidator<CreateLeaveTypeCommand>
{
    public CreateLeaveTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave type name is required")
            .MaximumLength(100).WithMessage("Leave type name must not exceed 100 characters");

        RuleFor(x => x.DefaultDays)
            .GreaterThan(0).WithMessage("Default days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Default days cannot exceed 365");
    }
}

public class UpdateLeaveTypeCommandValidator : AbstractValidator<UpdateLeaveTypeCommand>
{
    public UpdateLeaveTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave type name is required")
            .MaximumLength(100).WithMessage("Leave type name must not exceed 100 characters");

        RuleFor(x => x.DefaultDays)
            .GreaterThan(0).WithMessage("Default days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Default days cannot exceed 365");
    }
}
