using Microsoft.AspNetCore.Identity;

namespace App.Domain.Models;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public List<Sample> Samples { get; set; }
    public List<Notification> Notifications { get; set; }
    public List<UserTag> UserTags { get; set; }
}
