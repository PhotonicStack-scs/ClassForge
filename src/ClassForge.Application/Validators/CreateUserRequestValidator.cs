using ClassForge.Application.DTOs.Users;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "OrgAdmin" or "ScheduleManager" or "Viewer")
            .WithMessage("Role must be OrgAdmin, ScheduleManager, or Viewer.");
    }
}
