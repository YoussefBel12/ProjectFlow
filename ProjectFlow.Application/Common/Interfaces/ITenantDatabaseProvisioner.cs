using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Application.Common.Interfaces
{
    

    public interface ITenantDatabaseProvisioner
    {
        Task<string> CreateDatabaseAsync(
            string slug,
            CancellationToken cancellationToken);
    }
}
