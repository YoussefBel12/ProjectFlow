using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
namespace ProjectFlow.Api.Swagger
{
   

    public class AddTenantHeaderOperationFilter : IOperationFilter
    {
        public void Apply(
            OpenApiOperation operation,
            OperationFilterContext context)
        {
            operation.Parameters ??= [];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Slug",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Tenant slug, e.g. globex",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
