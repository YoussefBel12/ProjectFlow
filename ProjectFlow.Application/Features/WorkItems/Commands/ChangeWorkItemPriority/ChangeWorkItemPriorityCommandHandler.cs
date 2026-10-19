
using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;

public class ChangeWorkItemPriorityCommandHandler
    : IRequestHandler<ChangeWorkItemPriorityCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeWorkItemPriorityCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ChangeWorkItemPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _repository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        workItem.ChangePriority(request.Priority);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
