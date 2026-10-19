using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Infrastructure.Tenancy
{
    

    public interface ITenantContext
    {
        string? Slug { get; }
        string? ConnectionString { get; }

        void SetTenant(string slug, string connectionString);
    }
}
