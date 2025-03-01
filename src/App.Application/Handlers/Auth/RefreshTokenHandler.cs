using App.Application.Responses;
using App.Application.Services;
using App.Domain.Exceptions;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Auth;

public record RefreshTokenCommand(string refreshToken) : IRequest<TokenResponse>;

public class RefreshTokenHandler(
    IUserService userService,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByRefreshToken(request.refreshToken, cancellationToken);
        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new DomainException("Invalid refresh token.", (int)HttpStatusCode.Unauthorized);
        }

        var roles = await userService.GetRolesAsync(user);
        var newAccessToken = jwtTokenService.GenerateToken(user, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userService.UpdateAsync(user);

        return new TokenResponse(newAccessToken, newRefreshToken);
    }
}
