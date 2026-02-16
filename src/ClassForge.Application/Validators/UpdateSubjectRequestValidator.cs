using ClassForge.Application.DTOs.Subjects;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxPeriodsPerDay).GreaterThan(0);
        RuleFor(x => x.SpecialRoomId).NotNull().When(x => x.RequiresSpecialRoom)
            .WithMessage("SpecialRoomId is required when RequiresSpecialRoom is true.");
    }
}
