using App.Application.Responses;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace App.Application.Handlers.Auth;

public record RegisterUserCommand(string email, string username, string password) : IRequest<TokenResponse>;

public class RegisterUserHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtTokenSettings) : IRequestHandler<RegisterUserCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.username,
            Email = request.email,
            CreatedAt = DateTime.Now,
        };

        var result = await userManager.CreateAsync(user, request.password);
        if (!result.Succeeded)
        {
            throw new DomainException("User registration failed.", (int)HttpStatusCode.BadRequest);
        }

        await userManager.AddToRoleAsync(user, UserRoles.USER.ToString());

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(jwtTokenSettings.Value.RefreshTokenExpiryInDays);

        await userManager.UpdateAsync(user);

        return new TokenResponse(token, refreshToken);
    }
}
