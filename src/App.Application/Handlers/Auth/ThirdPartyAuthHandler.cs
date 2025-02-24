using App.Application.Extensions;
using App.Application.Responses;
using App.Application.Services;
using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Text;

namespace App.Application.Handlers.Auth;

public record ThirdPartyAuthCommand(string email, string? firstName, string? lastName, string? imgUrl) : IRequest<TokenResponse>;

public class ThirdPartyAuthHandler(
    UserManager<User> userManager,
    IJwtTokenService jwtTokenService) : IRequestHandler<ThirdPartyAuthCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(ThirdPartyAuthCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.email))
        {
            throw new DomainException($"Failed to retrieve Email.", (int)HttpStatusCode.BadRequest);
        }

        var user = await userManager.FindByEmailAsync(request.email);
        if (user == null)
        {
            string uniqueUsername;

            do
            {
                uniqueUsername = ApplicationExtensions.GenerateRandomUsername();
            }
            while (await userManager.FindByNameAsync(uniqueUsername) != null);

            user = new User
            {
                Email = request.email,
                FirstName = request.firstName,
                LastName = request.lastName,
                UserName = uniqueUsername,
                AvatarUrl = request.imgUrl,
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new DomainException("User registration failed.", (int)HttpStatusCode.InternalServerError);
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return new TokenResponse(token, refreshToken);
    }
}
