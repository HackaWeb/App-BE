using App.Application.Services;
using App.Domain.Exceptions;
using App.RestContracts.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Users;

public record GetUserByIdCommand(string userId) : IRequest<UserModel>;

public class GetUserByIdHandler(
    IUserService userService) : IRequestHandler<GetUserByIdCommand, UserModel>
{
    public async Task<UserModel> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(request.userId, cancellationToken);
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