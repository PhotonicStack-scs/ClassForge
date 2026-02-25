using ClassForge.Application.DTOs.Years;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateYearsRequestValidator : AbstractValidator<BulkCreateYearsRequest>
{
    public BulkCreateYearsRequestValidator(IValidator<CreateYearRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
