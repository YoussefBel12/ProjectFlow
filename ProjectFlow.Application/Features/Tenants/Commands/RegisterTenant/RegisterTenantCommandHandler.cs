using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
namespace ProjectFlow.Application.Features.Tenants.Commands.RegisterTenant
{
   

    public class RegisterTenantCommandHandler
        : IRequestHandler<RegisterTenantCommand, Guid>
    {
        private readonly ITenantStore _tenantStore;
        private readonly ITenantDatabaseProvisioner _databaseProvisioner;
        private readonly ITenantDatabaseInitializer _databaseInitializer;
        public RegisterTenantCommandHandler(
            ITenantStore tenantStore,
            ITenantDatabaseProvisioner databaseProvisioner,
            ITenantDatabaseInitializer databaseInitializer)
        {
            _tenantStore = tenantStore;
            _databaseProvisioner = databaseProvisioner;
            _databaseInitializer = databaseInitializer;
        }

        public async Task<Guid> Handle(
            RegisterTenantCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _tenantStore.GetBySlugAsync(
                request.Slug,
                cancellationToken);

            if (existing is not null)
                throw new InvalidOperationException(
                    "This tenant slug is already registered.");

            var connectionString =
    await _databaseProvisioner.CreateDatabaseAsync(
        request.Slug,
        cancellationToken);

            await _databaseInitializer.InitializeAsync(
                connectionString,
                cancellationToken);

            

                        var tenant = new Tenant(
                request.Name,
                request.Slug,
                connectionString);

            await _tenantStore.AddAsync(tenant, cancellationToken);

            return tenant.Id;
        }
    }
}
