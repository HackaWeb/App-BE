using App.Application.Responses;
using App.Application.Services;
using App.Domain.Exceptions;
using App.Domain.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net;

namespace App.Application.Handlers.Auth;

public record LoginUserCommand(string email, string password) : IRequest<TokenResponse>;

public class LoginUserHandler(
    IUserService userService,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtTokenSettings) : IRequestHandler<LoginUserCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByEmailAsync(request.email, cancellationToken);
        if (user is null || !await userService.CheckPasswordAsync(user, request.password))
        {
            throw new DomainException("Invalid email or password.", (int)HttpStatusCode.Unauthorized);
        }

        var roles = await userService.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtTokenSettings.Value.RefreshTokenExpiryInDays);
        await userService.UpdateAsync(user);

        return new TokenResponse(token, refreshToken);
    }
}
