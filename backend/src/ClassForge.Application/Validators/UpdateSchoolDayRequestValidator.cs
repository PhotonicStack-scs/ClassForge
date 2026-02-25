using ClassForge.Application.DTOs.SchoolDays;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateSchoolDayRequestValidator : AbstractValidator<UpdateSchoolDayRequest>
{
    public UpdateSchoolDayRequestValidator()
    {
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
