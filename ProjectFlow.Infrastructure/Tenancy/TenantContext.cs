using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Infrastructure.Tenancy
{
    

    public class TenantContext : ITenantContext
    {
        public string? Slug { get; private set; }
        public string? ConnectionString { get; private set; }

        public void SetTenant(string slug, string connectionString)
        {
            Slug = slug;
            ConnectionString = connectionString;
        }
    }
}
