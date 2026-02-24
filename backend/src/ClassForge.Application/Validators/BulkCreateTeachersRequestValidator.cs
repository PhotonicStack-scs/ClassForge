using ClassForge.Application.DTOs.Teachers;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class BulkCreateTeachersRequestValidator : AbstractValidator<BulkCreateTeachersRequest>
{
    public BulkCreateTeachersRequestValidator(IValidator<CreateTeacherRequest> itemValidator)
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(itemValidator);
    }
}
