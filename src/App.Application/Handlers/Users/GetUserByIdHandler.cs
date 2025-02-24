using App.Domain.Enums;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.RestContracts.Users.Models;
using App.RestContracts.Users.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record GetUserByIdCommand(string userId) : IRequest<UserModel>;

public class GetUserByIdHandler(
    UserManager<User> userManager) : IRequestHandler<GetUserByIdCommand, UserModel>
{
    public async Task<UserModel> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        var userRest = new UserModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };

        return userRest;
    }
}