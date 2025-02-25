using System.ComponentModel.DataAnnotations;

namespace App.Domain.Models;

public class Tag : BaseModel
{
    [Required]
    public string Name { get; set; }

    public virtual List<UserTag> UserTags { get; set; } = new List<UserTag>();
}