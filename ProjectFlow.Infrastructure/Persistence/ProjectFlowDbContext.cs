using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence
{
   

    public class ProjectFlowDbContext : DbContext
    {
        public ProjectFlowDbContext(
            DbContextOptions<ProjectFlowDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
        public DbSet<WorkItemComment> WorkItemComments => Set<WorkItemComment>();
        public DbSet<ProjectActivity> ProjectActivities => Set<ProjectActivity>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ProjectFlowDbContext).Assembly);
        }
    }
}
