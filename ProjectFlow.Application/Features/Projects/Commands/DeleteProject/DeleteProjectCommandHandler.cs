using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler
    : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (project is null)
            return false;

        _repository.RemoveProject(project);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}