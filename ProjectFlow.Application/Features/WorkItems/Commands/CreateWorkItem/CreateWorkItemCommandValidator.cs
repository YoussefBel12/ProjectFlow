using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
namespace ProjectFlow.Application.Features.WorkItems.Commands.CreateWorkItem
{

    

    public class CreateWorkItemCommandValidator
        : AbstractValidator<CreateWorkItemCommand>
    {
        public CreateWorkItemCommandValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.DueDateUtc)
                .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
                .WithMessage("Due date must be in the future.");
        }
    }

}
