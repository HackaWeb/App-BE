using App.Application.Repositories;
using App.Domain.Exceptions;
using App.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record DeleteUserImage(string userId) : IRequest;

public class DeleteUserImageHandler(
    IBlobStorageRepository blobRepository,
    UserManager<User> userManager) : IRequestHandler<DeleteUserImage>
{
    public async Task Handle(DeleteUserImage request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }
    }
}