using ClassForge.Application.DTOs.YearDayConfigs;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateYearDayConfigRequestValidator : AbstractValidator<CreateYearDayConfigRequest>
{
    public CreateYearDayConfigRequestValidator()
    {
        RuleFor(x => x.SchoolDayId).NotEmpty();
        RuleFor(x => x.MaxPeriods).GreaterThan(0);
    }
}
