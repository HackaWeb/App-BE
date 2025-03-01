using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.DataContext.Models
{
    [Table("Users", Schema = "dbo")]
    internal class User : IdentityUser<Guid>
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public virtual List<Notification>? Notifications { get; set; }
        public virtual List<UserTag>? UserTags { get; set; }
        public virtual ICollection<Credential> Credentials { get; set; } = new List<Credential>();
    }
}
