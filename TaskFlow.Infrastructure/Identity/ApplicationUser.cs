using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
}
