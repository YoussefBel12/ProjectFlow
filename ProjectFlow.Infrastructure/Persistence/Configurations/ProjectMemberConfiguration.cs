
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration
    : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(member => member.Id);

        builder.Property(member => member.Role)
            .IsRequired();

        builder.HasIndex(member => new
        {
            member.ProjectId,
            member.UserId
        })
        .IsUnique();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
