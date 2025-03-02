using App.Application.Extensions;
using App.Application.Repositories;
using App.Application.Responses;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Auth;

public record ThirdPartyAuthCommand(string email, string? firstName, string? lastName, string? imgUrl) : IRequest<TokenResponse>;

public class ThirdPartyAuthHandler(
    IUserService userService,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService) : IRequestHandler<ThirdPartyAuthCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(ThirdPartyAuthCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.email))
        {
            throw new DomainException($"Failed to retrieve Email.", (int)HttpStatusCode.BadRequest);
        }

        var user = await userService.GetByEmailAsync(request.email);
        if (user == null)
        {
            string uniqueUsername;

            do
            {
                uniqueUsername = ApplicationExtensions.GenerateRandomUsername();
            }
            while (await userService.IsUserNameExists(uniqueUsername));

            user = new User
            {
                Email = request.email,
                FirstName = request.firstName,
                LastName = request.lastName,
                UserName = uniqueUsername,
                AvatarUrl = request.imgUrl,
            };

            await userService.CreateAsync(user);
        }

        var roles = await userService.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        var transaction = new Transaction()
        {
            Amount = 10,
            TransactionDate = DateTime.UtcNow,
            Balance = 10,
            Type = TransactionType.Deposit,
            UserId = user.Id
        };

        await unitOfWork.TransactionRepository.Add(transaction);
        await userService.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync();

        return new TokenResponse(token, refreshToken);
    }
}
