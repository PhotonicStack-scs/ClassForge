using ClassForge.Application.DTOs.Curricula;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateYearCurriculaRequestValidator : AbstractValidator<BulkCreateYearCurriculaRequest>
{
    public BulkCreateYearCurriculaRequestValidator(IValidator<CreateYearCurriculumRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
