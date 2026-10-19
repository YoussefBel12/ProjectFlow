using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Infrastructure.Tenancy;

namespace ProjectFlow.Infrastructure.Tenancy
{




    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITenantStore tenantStore,
            ITenantContext tenantContext)
        {
            var path = context.Request.Path;

            if (!path.StartsWithSegments("/api") ||
                path.StartsWithSegments("/api/tenants"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(
                    TenantHeaderDefaults.HeaderName, out var slug) ||
                string.IsNullOrWhiteSpace(slug))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(
                    $"Missing {TenantHeaderDefaults.HeaderName} header.");
                return;
            }

            var tenant = await tenantStore.GetBySlugAsync(
                slug.ToString(),
                context.RequestAborted);

            if (tenant is null || !tenant.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Tenant not found.");
                return;
            }

            tenantContext.SetTenant(tenant.Slug, tenant.ConnectionString);

            await _next(context);
        }
    }
}
