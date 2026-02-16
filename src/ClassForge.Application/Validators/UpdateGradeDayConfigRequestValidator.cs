using ClassForge.Application.DTOs.GradeDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateGradeDayConfigRequestValidator : AbstractValidator<UpdateGradeDayConfigRequest>
{
    public UpdateGradeDayConfigRequestValidator()
    {
        RuleFor(x => x.MaxPeriods).GreaterThan(0);
    }
}
