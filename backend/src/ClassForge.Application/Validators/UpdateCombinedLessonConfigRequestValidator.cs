using ClassForge.Application.DTOs.CombinedLessons;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateCombinedLessonConfigRequestValidator : AbstractValidator<UpdateCombinedLessonConfigRequest>
{
    public UpdateCombinedLessonConfigRequestValidator()
    {
        RuleFor(x => x.MaxGroupsPerLesson).GreaterThan(1);
        RuleFor(x => x.GroupIds).NotEmpty().WithMessage("At least one group must be specified.");
    }
}
