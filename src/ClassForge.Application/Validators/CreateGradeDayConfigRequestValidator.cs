using ClassForge.Application.DTOs.GradeDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateGradeDayConfigRequestValidator : AbstractValidator<CreateGradeDayConfigRequest>
{
    public CreateGradeDayConfigRequestValidator()
    {
        RuleFor(x => x.TeachingDayId).NotEmpty();
        RuleFor(x => x.MaxPeriods).GreaterThan(0);
    }
}
