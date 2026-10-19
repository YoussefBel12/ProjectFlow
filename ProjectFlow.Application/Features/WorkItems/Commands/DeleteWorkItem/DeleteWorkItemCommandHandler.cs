using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.WorkItems.Commands.DeleteWorkItem;

public class DeleteWorkItemCommandHandler
    : IRequestHandler<DeleteWorkItemCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWorkItemCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteWorkItemCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _repository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        _repository.RemoveWorkItem(workItem);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}