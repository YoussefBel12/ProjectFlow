
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.CreateWorkItem;

public class CreateWorkItemCommandHandler
    : IRequestHandler<CreateWorkItemCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectActivityRepository _projectActivityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkItemCommandHandler(
        IProjectRepository projectRepository,
        IProjectActivityRepository projectActivityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _projectActivityRepository = projectActivityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateWorkItemCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user is required to create a work item.");
        }

        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            throw new KeyNotFoundException(
                $"Project with ID '{request.ProjectId}' was not found.");
        }

        var workItem = project.AddWorkItem(
            request.Title,
            request.Description,
            request.DueDateUtc);

        await _projectRepository.AddWorkItemAsync(
            workItem,
            cancellationToken);

        var activity = new ProjectActivity(
            project.Id,
            userId.Value,
            ActivityType.WorkItemCreated,
            $"Created work item '{request.Title}'",
            workItem.Id);

        await _projectActivityRepository.AddAsync(
            activity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workItem.Id;
    }
}
