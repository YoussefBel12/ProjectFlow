using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence
{
  

    public class TenantStore : ITenantStore
    {
        private readonly TenantCatalogDbContext _context;

        public TenantStore(TenantCatalogDbContext context)
        {
            _context = context;
        }

        public Task<Tenant?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken)
        {
            return _context.Tenants.FirstOrDefaultAsync(
                t => t.Slug == slug.ToLower(),
                cancellationToken);
        }

        public async Task AddAsync(
            Tenant tenant,
            CancellationToken cancellationToken)
        {
            await _context.Tenants.AddAsync(tenant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
