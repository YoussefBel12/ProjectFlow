using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
namespace ProjectFlow.Application.Features.WorkItems.Commands.CreateWorkItem
{

    

    public class CreateWorkItemCommandHandler
        : IRequestHandler<CreateWorkItemCommand, Guid>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkItemCommandHandler(
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(
            CreateWorkItemCommand request,
            CancellationToken cancellationToken)
        {
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return workItem.Id;
        }
    }

}
