using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record DeleteUserCommand(string userId) : IRequest;

public class DeleteUserHandler(UserManager<User> userManager) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user == null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        var deleteResult = await userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            throw new DomainException("Failed to delete user.", (int)HttpStatusCode.BadRequest, deleteResult.Errors.ToDictionary(x => x.Code, x => x.Description));
        }
    }
}