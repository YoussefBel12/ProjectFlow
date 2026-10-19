namespace ProjectFlow.Api.Contracts.Tenants
{
   

    public record RegisterTenantRequest(
        string Name,
        string Slug);
}
