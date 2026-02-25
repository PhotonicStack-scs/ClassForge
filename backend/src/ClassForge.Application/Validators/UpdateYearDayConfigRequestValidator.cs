using ClassForge.Application.DTOs.YearDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateYearDayConfigRequestValidator : AbstractValidator<UpdateYearDayConfigRequest>
{
    public UpdateYearDayConfigRequestValidator()
    {
        RuleFor(x => x.MaxPeriods).GreaterThan(0);
    }
}
