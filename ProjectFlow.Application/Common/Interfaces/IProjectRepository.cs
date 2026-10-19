using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces
{



    public interface IProjectRepository
    {
        Task AddAsync(
            Project project,
            CancellationToken cancellationToken);

        Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task AddWorkItemAsync(
            WorkItem workItem,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<WorkItem>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken);



        Task<WorkItem?> GetWorkItemByIdAsync(
    Guid id,
    CancellationToken cancellationToken);



    }


}