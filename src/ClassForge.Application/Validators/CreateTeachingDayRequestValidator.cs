using ClassForge.Application.DTOs.TeachingDays;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTeachingDayRequestValidator : AbstractValidator<CreateTeachingDayRequest>
{
    public CreateTeachingDayRequestValidator()
    {
        RuleFor(x => x.DayOfWeek).InclusiveBetween(0, 6);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
