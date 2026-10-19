using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;

namespace ProjectFlow.Domain.Entities
{
  

    public class Tenant : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string ConnectionString { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        private Tenant() { }

        public Tenant(string name, string slug, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name is required.");

            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Tenant slug is required.");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.");

            Name = name.Trim();
            Slug = slug.Trim().ToLowerInvariant();
            ConnectionString = connectionString;
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsUpdated();
        }
    }
}
