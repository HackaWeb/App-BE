namespace App.Domain.Models;

public class Tag : BaseModel
{
    public string Name { get; set; }

    public List<UserTag> UserTags { get; set; }
}