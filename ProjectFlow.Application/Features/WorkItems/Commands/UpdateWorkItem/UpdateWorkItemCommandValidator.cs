
using FluentValidation;

namespace ProjectFlow.Application.Features.WorkItems.Commands.UpdateWorkItem;

public class UpdateWorkItemCommandValidator
    : AbstractValidator<UpdateWorkItemCommand>
{
    public UpdateWorkItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.DueDateUtc)
            .Must(dueDate => !dueDate.HasValue ||
                             dueDate.Value > DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}
