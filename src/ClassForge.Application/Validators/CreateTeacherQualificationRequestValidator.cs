using ClassForge.Application.DTOs.TeacherQualifications;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTeacherQualificationRequestValidator : AbstractValidator<CreateTeacherQualificationRequest>
{
    public CreateTeacherQualificationRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.MinGradeId).NotEmpty();
        RuleFor(x => x.MaxGradeId).NotEmpty();
    }
}
