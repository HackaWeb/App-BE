namespace App.Domain.Models;

public class UserTag : BaseModel
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid TagId { get; set; }
    public Tag Tag { get; set; }
}