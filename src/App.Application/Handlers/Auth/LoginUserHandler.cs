using App.Application.Responses;
using App.Application.Services;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace App.Application.Handlers.Auth;

public record LoginUserCommand(string email, string password) : IRequest<TokenResponse>;

public class LoginUserHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtTokenSettings) : IRequestHandler<LoginUserCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.password))
        {
            throw new DomainException("Invalid email or password.", (int)HttpStatusCode.Unauthorized);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtTokenSettings.Value.RefreshTokenExpiryInDays);
        await userManager.UpdateAsync(user);

        return new TokenResponse(token, refreshToken);
    }
}
