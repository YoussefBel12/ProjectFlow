using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.Tenants;
using ProjectFlow.Application.Features.Tenants.Commands.RegisterTenant;

namespace ProjectFlow.Api.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ISender _sender;

        public TenantsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(
            RegisterTenantRequest request,
            CancellationToken cancellationToken)
        {
            var tenantId = await _sender.Send(
                new RegisterTenantCommand(request.Name, request.Slug),
                cancellationToken);

            return Created(
                $"/api/tenants/{tenantId}",
                new { id = tenantId });
        }
    }
}
