
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository repository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
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

        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        var membership = await _memberRepository.GetByProjectAndUserAsync(
            request.Id,
            userId.Value,
            cancellationToken);

        if (membership is null || membership.Role != ProjectRole.Manager)
            throw new UnauthorizedAccessException(
                "Only project managers can update this project.");

        project.UpdateDetails(
            request.Name,
            request.Description,
            request.DueDateUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
