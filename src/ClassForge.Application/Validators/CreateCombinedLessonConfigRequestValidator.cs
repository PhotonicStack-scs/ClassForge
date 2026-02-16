using ClassForge.Application.DTOs.CombinedLessons;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateCombinedLessonConfigRequestValidator : AbstractValidator<CreateCombinedLessonConfigRequest>
{
    public CreateCombinedLessonConfigRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.MaxGroupsPerLesson).GreaterThan(1);
        RuleFor(x => x.GroupIds).NotEmpty().WithMessage("At least one group must be specified.");
    }
}
