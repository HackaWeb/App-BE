using App.Application.Responses;
using App.Application.Services;
using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace App.Application.Handlers.Auth;

public record ThirdPartyAuthCommand(string scheme) : IRequest<TokenResponse>;

public class ThirdPartyAuthHandler(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    IJwtTokenService jwtTokenService) : IRequestHandler<ThirdPartyAuthCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(ThirdPartyAuthCommand request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;
        var authenticateResult = await context.AuthenticateAsync(request.scheme);

        if (!authenticateResult.Succeeded)
        {
            throw new DomainException("Authentication error", (int)HttpStatusCode.Unauthorized);
        }

        var email = authenticateResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var firstName = authenticateResult.Principal?.FindFirst(ClaimTypes.GivenName)?.Value;
        var surname = authenticateResult.Principal?.FindFirst(ClaimTypes.Surname)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            throw new DomainException($"Failed to retrieve Email from {request.scheme}", (int)HttpStatusCode.BadRequest);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            string uniqueUsername;

            do
            {
                uniqueUsername = GenerateRandomUsername();
            }
            while (await userManager.FindByNameAsync(uniqueUsername) != null);

            user = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = surname,
                UserName = uniqueUsername,
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

    private string GenerateRandomUsername()
    {
        var random = new Random();
        var sb = new StringBuilder();
        sb.Append("koala_");
        for (int i = 0; i < 8; i++)
        {
            sb.Append(random.Next(0, 10));
        }
        return sb.ToString();
    }
}
