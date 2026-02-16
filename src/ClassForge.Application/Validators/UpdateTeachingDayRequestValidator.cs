using ClassForge.Application.DTOs.TeachingDays;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTeachingDayRequestValidator : AbstractValidator<UpdateTeachingDayRequest>
{
    public UpdateTeachingDayRequestValidator()
    {
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
