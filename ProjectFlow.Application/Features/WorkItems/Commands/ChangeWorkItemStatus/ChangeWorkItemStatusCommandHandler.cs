
using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemStatus;

public class ChangeWorkItemStatusCommandHandler
    : IRequestHandler<ChangeWorkItemStatusCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeWorkItemStatusCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ChangeWorkItemStatusCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        workItem.ChangeStatus(request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
