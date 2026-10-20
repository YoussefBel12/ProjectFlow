using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientUserId)
            .IsRequired();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.ReadAtUtc)
            .IsRequired(false);

        builder.HasIndex(n => new
        {
            n.RecipientUserId,
            n.ReadAtUtc,
            n.CreatedAtUtc
        });
    }
}