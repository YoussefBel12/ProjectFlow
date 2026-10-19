using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
namespace ProjectFlow.Application.Features.Tenants.Commands.RegisterTenant
{
    

   

    public class RegisterTenantCommandValidator
        : AbstractValidator<RegisterTenantCommand>
    {
        public RegisterTenantCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Slug)
                .NotEmpty()
                .MaximumLength(100)
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage(
                    "Slug must contain lowercase letters, numbers, or hyphens.");
        }
    }
}
