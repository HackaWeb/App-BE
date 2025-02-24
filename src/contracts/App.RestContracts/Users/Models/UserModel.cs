namespace App.RestContracts.Users.Models;

public class UserModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}