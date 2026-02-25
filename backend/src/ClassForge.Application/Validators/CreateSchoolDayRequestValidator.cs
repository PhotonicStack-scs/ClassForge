using ClassForge.Application.DTOs.SchoolDays;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateSchoolDayRequestValidator : AbstractValidator<CreateSchoolDayRequest>
{
    public CreateSchoolDayRequestValidator()
    {
        RuleFor(x => x.DayOfWeek).InclusiveBetween(0, 6);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
