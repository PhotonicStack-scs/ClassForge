using ClassForge.Application.DTOs.Timetables;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class CreateTimetableRequestValidator : AbstractValidator<CreateTimetableRequest>
{
    public CreateTimetableRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
