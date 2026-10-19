using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Infrastructure.Persistence.Repositories
{
  

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProjectFlowDbContext _context;

        public UnitOfWork(ProjectFlowDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
