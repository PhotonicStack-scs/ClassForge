using ClassForge.Application.DTOs.Grades;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateGradesRequestValidator : AbstractValidator<BulkCreateGradesRequest>
{
    public BulkCreateGradesRequestValidator(IValidator<CreateGradeRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
