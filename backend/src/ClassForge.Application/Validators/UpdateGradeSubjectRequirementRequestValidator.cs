using ClassForge.Application.DTOs.GradeSubjectRequirements;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateGradeSubjectRequirementRequestValidator : AbstractValidator<UpdateGradeSubjectRequirementRequest>
{
    public UpdateGradeSubjectRequirementRequestValidator()
    {
        RuleFor(x => x.PeriodsPerWeek).GreaterThan(0);
        RuleFor(x => x.MaxPeriodsPerDay).GreaterThan(0);
    }
}
