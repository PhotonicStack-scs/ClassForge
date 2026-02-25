using ClassForge.Application.DTOs.Timetables;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTimetableEntryRequestValidator : AbstractValidator<UpdateTimetableEntryRequest>
{
    public UpdateTimetableEntryRequestValidator()
    {
        RuleFor(x => x.TimeSlotId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.ClassIds).NotEmpty();
        RuleForEach(x => x.ClassIds).NotEmpty();
    }
}
