namespace App.Domain.Models;

public class User : BaseModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public List<Notification> Notifications { get; set; }
    public List<UserTag> UserTags { get; set; }
}
