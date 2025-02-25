using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Domain.Models;

public class UserTag : BaseModel
{
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    [Required]
    public Guid TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public virtual Tag Tag { get; set; }
}