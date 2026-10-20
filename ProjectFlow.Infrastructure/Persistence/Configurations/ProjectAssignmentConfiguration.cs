
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Configurations;

public sealed class ProjectAssignmentConfiguration
    : IEntityTypeConfiguration<ProjectAssignment>
{
    public void Configure(EntityTypeBuilder<ProjectAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.WorkItemId, a.UserId })
            .IsUnique();

        builder.HasIndex(a => a.UserId);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
