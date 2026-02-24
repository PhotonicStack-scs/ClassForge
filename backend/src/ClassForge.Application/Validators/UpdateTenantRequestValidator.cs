using ClassForge.Application.DTOs.Tenants;
using FluentValidation;

namespace ClassForge.Application.Validators;

public class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultLanguage).MaximumLength(10).When(x => x.DefaultLanguage is not null);
    }
}
