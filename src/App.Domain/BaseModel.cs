using System.ComponentModel.DataAnnotations;

namespace App.Domain;

public class BaseModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}
