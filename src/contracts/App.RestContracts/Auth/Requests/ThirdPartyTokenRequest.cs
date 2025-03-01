namespace App.RestContracts.Auth.Requests;

public record ThirdPartyTokenRequest
{
    public string Token { get; init; }
}