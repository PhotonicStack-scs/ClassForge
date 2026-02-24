using ClassForge.Application.DTOs.Teachers;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTeacherRequestValidator : AbstractValidator<CreateTeacherRequest>
{
    public CreateTeacherRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null).MaximumLength(256);
    }
}
