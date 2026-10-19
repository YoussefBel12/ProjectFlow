using Microsoft.AspNetCore.Identity;

namespace ProjectFlow.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
}