using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Application.Features.Tenants.Commands.RegisterTenant
{
    using MediatR;


    public record RegisterTenantCommand(
        string Name,
        string Slug
    ) : IRequest<Guid>;
}
