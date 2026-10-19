using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
namespace ProjectFlow.Infrastructure.Persistence
{
   

    public class TenantDatabaseInitializer : ITenantDatabaseInitializer
    {
        public async Task InitializeAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            var options = new DbContextOptionsBuilder<ProjectFlowDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new ProjectFlowDbContext(options);

            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
