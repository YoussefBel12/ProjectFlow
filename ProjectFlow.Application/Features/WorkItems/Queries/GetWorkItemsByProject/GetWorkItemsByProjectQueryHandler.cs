using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemsByProject
{

   

    public class GetWorkItemsByProjectQueryHandler
        : IRequestHandler<GetWorkItemsByProjectQuery, IReadOnlyList<WorkItem>>
    {
        private readonly IProjectRepository _projectRepository;

        public GetWorkItemsByProjectQueryHandler(
            IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IReadOnlyList<WorkItem>> Handle(
            GetWorkItemsByProjectQuery request,
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

            return await _projectRepository.GetByProjectIdAsync(
                request.ProjectId,
                cancellationToken);
        }
    }

}
