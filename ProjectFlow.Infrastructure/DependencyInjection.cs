

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Infrastructure.Identity;
using ProjectFlow.Infrastructure.Persistence;
using ProjectFlow.Infrastructure.Persistence.Repositories;
using ProjectFlow.Infrastructure.Tenancy;

namespace ProjectFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ProjectFlowDbContext>(
                (serviceProvider, options) =>
                {
                    var tenantContext =
                        serviceProvider.GetRequiredService<ITenantContext>();

                    var connectionString = tenantContext.ConnectionString;

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException(
                            "Tenant database connection has not been resolved.");
                    }

                    options.UseSqlServer(connectionString);
                });

            services.AddDbContext<TenantCatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CatalogConnection")));



            services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("IdentityConnection")));

            services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<IdentityDbContext>();


            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<
    IProjectAssignmentRepository,
    ProjectAssignmentRepository>();
            services.AddScoped<IProjectActivityRepository, ProjectActivityRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ITenantStore, TenantStore>();
            services.AddScoped<
                ITenantDatabaseProvisioner,
                TenantDatabaseProvisioner>();
            services.AddScoped<IWorkItemCommentRepository, WorkItemCommentRepository>();
            services.AddScoped<
                ITenantDatabaseInitializer,
                TenantDatabaseInitializer>();
            services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
            services.AddScoped<ITenantContext, TenantContext>();

            return services;
        }
    }
}
