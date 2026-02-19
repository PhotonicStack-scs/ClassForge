using ClassForge.Application.DTOs.Subjects;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateSubjectsRequestValidator : AbstractValidator<BulkCreateSubjectsRequest>
{
    public BulkCreateSubjectsRequestValidator(IValidator<CreateSubjectRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
