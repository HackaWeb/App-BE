using App.Application.Repositories;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.RestContracts.Users.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace App.Application.Handlers.Users;

public record UploadUserImageCommand(string userId, IFormFile file) : IRequest<UserImageResponse>;

public class UploadUserImageHandler(
    UserManager<User> userManager,
    IBlobStorageRepository repository) : IRequestHandler<UploadUserImageCommand, UserImageResponse>
{
    public async Task<UserImageResponse> Handle(UploadUserImageCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        var allowedFormats = new HashSet<string> { "image/png", "image/jpeg" };
        if (!allowedFormats.Contains(request.file.ContentType))
        {
            throw new DomainException("Only PNG and JPG images are allowed.", (int)HttpStatusCode.BadRequest);
        }

        var extension = request.file.ContentType == "image/png" ? "png" : "jpg";
        var fileName = $"{user.Id}.{extension}";

        using var stream = request.file.OpenReadStream();
        user.AvatarUrl = await repository.UploadAsync(stream, fileName, request.file.ContentType, "media");

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new DomainException("User info update ended with an error", (int)HttpStatusCode.InternalServerError);
        }

        return new UserImageResponse
        {
            AvatarUrl = user.AvatarUrl,
        };
    }
}