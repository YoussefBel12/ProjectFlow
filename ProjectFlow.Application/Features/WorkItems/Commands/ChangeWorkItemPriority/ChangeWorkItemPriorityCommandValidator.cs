
using FluentValidation;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;

public class ChangeWorkItemPriorityCommandValidator
    : AbstractValidator<ChangeWorkItemPriorityCommand>
{
    public ChangeWorkItemPriorityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .IsInEnum();
    }
}
