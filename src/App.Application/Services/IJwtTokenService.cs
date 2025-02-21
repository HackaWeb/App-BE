using App.Domain.Enums;
using App.Domain.Models;

namespace App.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user, IList<UserRoles> roles);

    string GenerateRefreshToken();

    bool ValidateRefreshToken(string refreshToken, string storedToken);
}
