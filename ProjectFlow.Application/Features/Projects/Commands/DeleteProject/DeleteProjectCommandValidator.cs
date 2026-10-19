using FluentValidation;

namespace ProjectFlow.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandValidator
    : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}