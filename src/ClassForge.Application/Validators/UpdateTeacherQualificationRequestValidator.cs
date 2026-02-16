using ClassForge.Application.DTOs.TeacherQualifications;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTeacherQualificationRequestValidator : AbstractValidator<UpdateTeacherQualificationRequest>
{
    public UpdateTeacherQualificationRequestValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.MinGradeId).NotEmpty();
        RuleFor(x => x.MaxGradeId).NotEmpty();
    }
}
