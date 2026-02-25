using ClassForge.Application.DTOs.Curricula;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateYearCurriculumRequestValidator : AbstractValidator<CreateYearCurriculumRequest>
{
    public CreateYearCurriculumRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.PeriodsPerWeek).GreaterThan(0);
        RuleFor(x => x.MaxPeriodsPerDay).GreaterThan(0);
    }
}
