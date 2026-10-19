using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            await _context.Projects.AddAsync(
                project,
                cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(
                    project => project.Id == id,
                    cancellationToken);
        }


        public async Task AddWorkItemAsync(
    WorkItem workItem,
    CancellationToken cancellationToken)
        {
            await _context.WorkItems.AddAsync(workItem, cancellationToken);
        }



        public async Task<IReadOnlyList<WorkItem>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            return await _context.WorkItems
                .AsNoTracking()
                .Where(workItem => workItem.ProjectId == projectId)
                .OrderBy(workItem => workItem.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkItem?> GetWorkItemByIdAsync(
    Guid id,
    CancellationToken cancellationToken)
        {
            return await _context.WorkItems
                
                .FirstOrDefaultAsync(
                    workItem => workItem.Id == id,
                    cancellationToken);
        }


        public void RemoveWorkItem(WorkItem workItem)
        {
            _context.WorkItems.Remove(workItem);
        }

        public void RemoveProject(Project project)
        {
            _context.Projects.Remove(project);
        }


        public async Task<IReadOnlyList<Project>> GetAllAsync(
    CancellationToken cancellationToken)
        {
            return await _context.Projects
                .AsNoTracking()
                .OrderBy(project => project.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }



    }
}
