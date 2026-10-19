using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProjectFlow.Infrastructure.Persistence;

namespace ProjectFlow.Infrastructure;

public class ProjectFlowDbContextFactory
    : IDesignTimeDbContextFactory<ProjectFlowDbContext>
{
    public ProjectFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "PROJECTFLOW_MIGRATION_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set PROJECTFLOW_MIGRATION_CONNECTION before creating migrations.");
        }

        var options = new DbContextOptionsBuilder<ProjectFlowDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ProjectFlowDbContext(options);
    }
}