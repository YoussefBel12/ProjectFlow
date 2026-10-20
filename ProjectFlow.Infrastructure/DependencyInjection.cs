
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Infrastructure.BackgroundJobs;
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
            // Tenant-specific database
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

            // Central tenant catalog database
            services.AddDbContext<TenantCatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("CatalogConnection")));

            // Central Identity database
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityConnection")));

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<IdentityDbContext>();

            // Hangfire background job infrastructure
            var hangfireConnection =
                configuration.GetConnectionString("HangfireConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'HangfireConnection' is missing.");

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireConnection));

            services.AddHangfireServer();


            services.AddScoped<
    IBackgroundJobService,
    BackgroundJobService>();

            services.AddScoped<
    INotificationCleanupJob,
    NotificationCleanupJob>();

            // Repositories and application infrastructure
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<
                IProjectAssignmentRepository,
                ProjectAssignmentRepository>();
            services.AddScoped<
                IProjectActivityRepository,
                ProjectActivityRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ITenantStore, TenantStore>();
            services.AddScoped<
                ITenantDatabaseProvisioner,
                TenantDatabaseProvisioner>();
            services.AddScoped<
                IWorkItemCommentRepository,
                WorkItemCommentRepository>();
            services.AddScoped<
                ITenantDatabaseInitializer,
                TenantDatabaseInitializer>();
            services.AddScoped<
                IProjectMemberRepository,
                ProjectMemberRepository>();
            services.AddScoped<ITenantContext, TenantContext>();

            return services;
        }
    }
}
