using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        /*  public async Task SaveChangesAsync(
              CancellationToken cancellationToken)
          {
              await _context.SaveChangesAsync(cancellationToken);
          } */

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Concurrency error: {entry.Metadata.ClrType.Name}, " +
                        $"State: {entry.State}, " +
                        $"ID: {entry.Property("Id").CurrentValue}");
                }

                throw;
            }
        }



    }
}
