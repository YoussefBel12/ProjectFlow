using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Commands.CreateProject
{
   

    public class CreateProjectCommandHandler
        : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProjectCommandHandler(
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(
            CreateProjectCommand request,
            CancellationToken cancellationToken)
        {
            var project = new Project(
                request.Name,
                request.Description,
                request.DueDateUtc);

            await _projectRepository.AddAsync(project, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return project.Id;
        }
    }
}
