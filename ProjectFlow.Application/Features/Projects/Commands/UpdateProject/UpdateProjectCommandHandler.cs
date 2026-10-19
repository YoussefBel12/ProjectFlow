using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (project is null)
            return false;

        project.UpdateDetails(
            request.Name,
            request.Description,
            request.DueDateUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}