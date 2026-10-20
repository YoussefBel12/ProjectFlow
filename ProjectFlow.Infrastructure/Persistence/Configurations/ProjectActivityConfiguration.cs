
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Configurations;

public sealed class ProjectActivityConfiguration
    : IEntityTypeConfiguration<ProjectActivity>
{
    public void Configure(EntityTypeBuilder<ProjectActivity> builder)
    {
        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Details)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(activity => activity.Type)
            .IsRequired();

        builder.HasIndex(activity => new
        {
            activity.ProjectId,
            activity.CreatedAtUtc
        });

        builder.HasIndex(activity => activity.WorkItemId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(activity => activity.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(activity => activity.WorkItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
