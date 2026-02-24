using ClassForge.Application.DTOs.TimeSlots;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTimeSlotRequestValidator : AbstractValidator<UpdateTimeSlotRequest>
{
    public UpdateTimeSlotRequestValidator()
    {
        RuleFor(x => x.SlotNumber).GreaterThan(0);
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x).Must(x =>
            TimeOnly.TryParse(x.StartTime, out var start) &&
            TimeOnly.TryParse(x.EndTime, out var end) &&
            start < end)
            .WithMessage("StartTime must be before EndTime.");
    }
}
