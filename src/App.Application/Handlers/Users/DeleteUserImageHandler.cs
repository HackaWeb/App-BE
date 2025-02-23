using App.Application.Repositories;
using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record DeleteUserImageCommand(string userId) : IRequest;

public class DeleteUserImageHandler(
    IBlobStorageRepository blobRepository,
    UserManager<User> userManager) : IRequestHandler<DeleteUserImageCommand>
{
    public async Task Handle(DeleteUserImageCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        var fileName = Path.GetFileName(user.AvatarUrl);

        var isDeleted = await blobRepository.DeleteAsync(fileName, "media");

        if (!isDeleted)
        {
            throw new DomainException("Failed to delete the image", (int)HttpStatusCode.InternalServerError);
        }

        user.AvatarUrl = null;
        await userManager.UpdateAsync(user);
    }
}