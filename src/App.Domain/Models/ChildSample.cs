namespace App.Domain.Models;

public class ChildSample : BaseModel
{
    public string ChildTitle { get; set; }

    public Sample Sample { get; set; }
    public Guid SampleId { get; set; }
}
