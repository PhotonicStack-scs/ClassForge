using ClassForge.Application.DTOs.GradeSubjectRequirements;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateGradeSubjectRequirementsRequestValidator : AbstractValidator<BulkCreateGradeSubjectRequirementsRequest>
{
    public BulkCreateGradeSubjectRequirementsRequestValidator(IValidator<CreateGradeSubjectRequirementRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
