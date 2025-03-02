namespace App.RestContracts.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public decimal Balance { get; set; }
    public bool IsAdmin { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<KeyModel> Keys { get; set; } = new List<KeyModel>();
}