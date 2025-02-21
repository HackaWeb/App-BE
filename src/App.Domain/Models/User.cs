using Microsoft.AspNetCore.Identity;

namespace App.Domain.Models;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
