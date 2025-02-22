namespace App.Domain.Settings;

public record JwtSettings
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public string Secret { get; init; }
    public ushort ExpiryMinutes { get; init; }
    public ushort RefreshTokenExpiryInDays { get; set; }
}
