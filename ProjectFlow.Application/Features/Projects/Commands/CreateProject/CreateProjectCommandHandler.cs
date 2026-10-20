
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectActivityRepository _projectActivityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IProjectActivityRepository projectActivityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectActivityRepository = projectActivityRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user is required to create a project.");
        }

        var project = new Project(
            request.Name,
            request.Description,
            request.DueDateUtc);

        var membership = new ProjectMember(
            project.Id,
            userId.Value,
            ProjectRole.Manager);

        var activity = new ProjectActivity(
            project.Id,
            userId.Value,
            ActivityType.ProjectCreated,
            $"Created project '{request.Name}'");

        await _projectRepository.AddAsync(project, cancellationToken);

        await _projectMemberRepository.AddAsync(
            membership,
            cancellationToken);

        await _projectActivityRepository.AddAsync(
            activity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
