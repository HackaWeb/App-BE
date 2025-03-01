using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.DataContext.Models;

[Table("Notifications", Schema = "dbo")]
internal class Notification : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [Required]
    public Guid SenderId { get; set; }

    [ForeignKey(nameof(SenderId))]
    public virtual User? Sender { get; set; }
}