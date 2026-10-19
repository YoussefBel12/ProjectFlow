using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Repositories
{
   

    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectFlowDbContext _context;

        public ProjectRepository(ProjectFlowDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Project project,
            CancellationToken cancellationToken)
        {
            await _context.Projects.AddAsync(project, cancellationToken);
        }
    }
}
