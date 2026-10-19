using FluentValidation;

namespace ProjectFlow.Application.Features.WorkItems.Commands.DeleteWorkItem;

public class DeleteWorkItemCommandValidator
    : AbstractValidator<DeleteWorkItemCommand>
{
    public DeleteWorkItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}