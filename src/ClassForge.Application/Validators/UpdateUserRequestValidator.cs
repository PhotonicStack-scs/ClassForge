using ClassForge.Application.DTOs.Users;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "OrgAdmin" or "ScheduleManager" or "Viewer")
            .WithMessage("Role must be OrgAdmin, ScheduleManager, or Viewer.");
    }
}
