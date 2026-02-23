using ClassForge.Application.DTOs.GradeSubjectRequirements;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateGradeSubjectRequirementRequestValidator : AbstractValidator<CreateGradeSubjectRequirementRequest>
{
    public CreateGradeSubjectRequirementRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.PeriodsPerWeek).GreaterThan(0);
        RuleFor(x => x.MaxPeriodsPerDay).GreaterThan(0);
    }
}
