
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Configurations;

public class WorkItemCommentConfiguration
    : IEntityTypeConfiguration<WorkItemComment>
{
    public void Configure(EntityTypeBuilder<WorkItemComment> builder)
    {
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(comment => comment.WorkItemId);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(comment => comment.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
