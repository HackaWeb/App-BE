namespace App.Domain;

public class BaseModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
