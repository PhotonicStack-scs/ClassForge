using ClassForge.Application.DTOs.Years;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateYearRequestValidator : AbstractValidator<CreateYearRequest>
{
    public CreateYearRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
