using FluentValidation;
using LeaveManagement.Api.Controllers;

namespace LeaveManagement.Application.Features.Auth.Validators;

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .EmailAddress().WithMessage("A valid email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MinimumLength(6).WithMessage("{PropertyName} must be at least 6 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("{PropertyName} is required");
    }
}
