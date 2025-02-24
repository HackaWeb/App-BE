using App.Domain.Enums;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.RestContracts.Users.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record UpdateUserCommand(string userId, string? firstName, string? lastName, string? email, string? username) : IRequest<UserModel>;

public class UpdateUserHandler(UserManager<User> userManager) : IRequestHandler<UpdateUserCommand, UserModel>
{
    public async Task<UserModel> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        if (!string.IsNullOrWhiteSpace(request.email))
            user.Email = request.email;
        
        if (!string.IsNullOrWhiteSpace(request.firstName))
            user.FirstName = request.firstName;
        
        if (!string.IsNullOrWhiteSpace(request.lastName))
            user.LastName = request.lastName;
        
        if (!string.IsNullOrWhiteSpace(request.username))
            user.LastName = request.username;

        var isAdmin = await userManager.IsInRoleAsync(user, nameof(UserRoles.ADMIN));
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new DomainException("User info update ended with an error", (int)HttpStatusCode.InternalServerError);
        }

        return new UserModel{
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }
}