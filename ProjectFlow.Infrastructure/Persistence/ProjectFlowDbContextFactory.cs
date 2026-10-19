using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProjectFlow.Infrastructure.Persistence
{
   

    public class ProjectFlowDbContextFactory
        : IDesignTimeDbContextFactory<ProjectFlowDbContext>
    {
        public ProjectFlowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProjectFlowDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=TOMMY\\SQLEXPRESS;Database=ProjectFlowDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new ProjectFlowDbContext(optionsBuilder.Options);
        }
    }
}
