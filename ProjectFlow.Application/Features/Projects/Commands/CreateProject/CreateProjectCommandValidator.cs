using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace ProjectFlow.Application.Features.Projects.Commands.CreateProject
{
   

    public class CreateProjectCommandValidator
        : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.DueDateUtc)
                .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
                .WithMessage("The due date must be in the future.");
        }
    }
}
