using ClassForge.Application.DTOs.CombinedLessons;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateCombinedLessonConfigRequestValidator : AbstractValidator<UpdateCombinedLessonConfigRequest>
{
    public UpdateCombinedLessonConfigRequestValidator()
    {
        RuleFor(x => x.MaxClassesPerLesson).GreaterThan(1);
        RuleFor(x => x.ClassIds).NotEmpty().WithMessage("At least one class must be specified.");
    }
}
