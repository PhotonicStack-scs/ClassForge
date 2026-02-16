using ClassForge.Application.DTOs.TeacherDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTeacherDayConfigRequestValidator : AbstractValidator<UpdateTeacherDayConfigRequest>
{
    public UpdateTeacherDayConfigRequestValidator()
    {
        RuleFor(x => x.MaxPeriods).GreaterThanOrEqualTo(0);
    }
}
