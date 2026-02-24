using ClassForge.Application.DTOs.TimeSlots;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateTimeSlotsRequestValidator : AbstractValidator<BulkCreateTimeSlotsRequest>
{
    public BulkCreateTimeSlotsRequestValidator(IValidator<CreateTimeSlotRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
