using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Infrastructure.Persistence
{
   

    public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
    {
        private readonly string _catalogConnectionString;

        public TenantDatabaseProvisioner(IConfiguration configuration)
        {
            _catalogConnectionString =
                configuration.GetConnectionString("CatalogConnection")
                ?? throw new InvalidOperationException(
                    "CatalogConnection is missing.");
        }

        public async Task<string> CreateDatabaseAsync(
            string slug,
            CancellationToken cancellationToken)
        {
            var databaseName = $"ProjectFlowTenant_{slug}";

            var builder = new SqlConnectionStringBuilder(
                _catalogConnectionString);

            builder.InitialCatalog = "master";

            await using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync(cancellationToken);

                var safeDatabaseName = databaseName.Replace("]", "]]");

                await using var command = connection.CreateCommand();

                command.CommandText = $"""
                IF DB_ID(@databaseName) IS NULL
                    CREATE DATABASE [{safeDatabaseName}];
                """;

                command.Parameters.AddWithValue("@databaseName", databaseName);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            var tenantBuilder = new SqlConnectionStringBuilder(
                _catalogConnectionString)
            {
                InitialCatalog = databaseName
            };

            return tenantBuilder.ConnectionString;
        }
    }
}
