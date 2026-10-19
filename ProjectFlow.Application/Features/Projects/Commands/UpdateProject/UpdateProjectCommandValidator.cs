using FluentValidation;

namespace ProjectFlow.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.DueDateUtc)
            .Must(dueDate => dueDate == null || dueDate > DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}