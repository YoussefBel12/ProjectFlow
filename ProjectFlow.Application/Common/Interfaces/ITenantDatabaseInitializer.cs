using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Application.Common.Interfaces
{
  

    public interface ITenantDatabaseInitializer
    {
        Task InitializeAsync(
            string connectionString,
            CancellationToken cancellationToken);
    }
}
