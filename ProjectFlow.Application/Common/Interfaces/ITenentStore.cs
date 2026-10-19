using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces
{
  

    public interface ITenantStore
    {
        Task<Tenant?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken);

        Task AddAsync(
            Tenant tenant,
            CancellationToken cancellationToken);
    }
}
