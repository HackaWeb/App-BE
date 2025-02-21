namespace App.RestContracts.Auth.Requests;

public class LoginUserRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
