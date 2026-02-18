using ClassForge.Application.DTOs.Timetables;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTimetableRequestValidator : AbstractValidator<UpdateTimetableRequest>
{
    public UpdateTimetableRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
