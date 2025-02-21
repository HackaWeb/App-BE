namespace App.Domain.Models;

public class Sample : BaseModel
{
    public string Title { get; set; }

    public List<ChildSample> ChildSamples { get; set; }
}
