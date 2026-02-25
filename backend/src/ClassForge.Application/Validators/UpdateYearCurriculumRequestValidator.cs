using ClassForge.Application.DTOs.Curricula;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateYearCurriculumRequestValidator : AbstractValidator<UpdateYearCurriculumRequest>
{
    public UpdateYearCurriculumRequestValidator()
    {
        RuleFor(x => x.PeriodsPerWeek).GreaterThan(0);
        RuleFor(x => x.MaxPeriodsPerDay).GreaterThan(0);
    }
}
