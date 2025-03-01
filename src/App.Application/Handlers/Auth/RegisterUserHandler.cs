using App.Application.Extensions;
using App.Application.Responses;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Models;
using App.Domain.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace App.Application.Handlers.Auth;

public record RegisterUserCommand(string email, string password) : IRequest<TokenResponse>;

public class RegisterUserHandler(
    IUserService userService,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtTokenSettings) : IRequestHandler<RegisterUserCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = ApplicationExtensions.GenerateRandomUsername(),
            Email = request.email,
            CreatedAt = DateTime.Now,
        };

        await userService.CreateAsync(user, request.password);
        await userService.AddToRoleAsync(user, UserRoles.USER.ToString());

        var roles = await userService.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(jwtTokenSettings.Value.RefreshTokenExpiryInDays);

        await userService.UpdateAsync(user);
        return new TokenResponse(token, refreshToken);
    }
}
