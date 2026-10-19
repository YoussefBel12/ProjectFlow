
using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.WorkItems.Commands.UpdateWorkItem;

public class UpdateWorkItemCommandHandler
    : IRequestHandler<UpdateWorkItemCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkItemCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateWorkItemCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        workItem.UpdateDetails(
            request.Title,
            request.Description,
            request.DueDateUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
