namespace App.Domain.Models;

public class Sample : BaseModel
{
    public string Title { get; set; }

    public User User { get; set; }
    public Guid UserId { get; set; }
    public List<ChildSample> ChildSamples { get; set; }
}
