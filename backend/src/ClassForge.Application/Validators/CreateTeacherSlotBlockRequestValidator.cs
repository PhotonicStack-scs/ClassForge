using ClassForge.Application.DTOs.TeacherSlotBlocks;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTeacherSlotBlockRequestValidator : AbstractValidator<CreateTeacherSlotBlockRequest>
{
    public CreateTeacherSlotBlockRequestValidator()
    {
        RuleFor(x => x.TimeSlotId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
