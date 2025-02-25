using System.ComponentModel.DataAnnotations;

namespace App.Domain.Models;

public class Notification : BaseModel
{
    [Required]
    public string Message { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public User User { get; set; }
    public Guid UserId { get; set; }

    public Guid SenderId { get; set; }
    public User Sender { get; set; }
}