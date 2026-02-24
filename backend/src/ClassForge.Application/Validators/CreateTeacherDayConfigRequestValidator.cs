using ClassForge.Application.DTOs.TeacherDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTeacherDayConfigRequestValidator : AbstractValidator<CreateTeacherDayConfigRequest>
{
    public CreateTeacherDayConfigRequestValidator()
    {
        RuleFor(x => x.TeachingDayId).NotEmpty();
        RuleFor(x => x.MaxPeriods).GreaterThanOrEqualTo(0);
    }
}
