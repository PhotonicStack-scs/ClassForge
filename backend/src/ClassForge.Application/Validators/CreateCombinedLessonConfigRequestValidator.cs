using ClassForge.Application.DTOs.CombinedLessons;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateCombinedLessonConfigRequestValidator : AbstractValidator<CreateCombinedLessonConfigRequest>
{
    public CreateCombinedLessonConfigRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.MaxClassesPerLesson).GreaterThan(1);
        RuleFor(x => x.ClassIds).NotEmpty().WithMessage("At least one class must be specified.");
    }
}
