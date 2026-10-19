using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence
{
    

    public class TenantCatalogDbContext : DbContext
    {
        public TenantCatalogDbContext(
            DbContextOptions<TenantCatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
                entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();
                entity.HasIndex(t => t.Slug).IsUnique();
                entity.Property(t => t.ConnectionString).IsRequired();
            });
        }
    }
}
