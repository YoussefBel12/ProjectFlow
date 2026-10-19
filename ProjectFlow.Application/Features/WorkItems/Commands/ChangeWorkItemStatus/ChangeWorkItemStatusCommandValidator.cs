
using FluentValidation;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemStatus;

public class ChangeWorkItemStatusCommandValidator
    : AbstractValidator<ChangeWorkItemStatusCommand>
{
    public ChangeWorkItemStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
