using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Exceptions;
using App.RestContracts.Users.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace App.Application.Handlers.Users;

public record UploadUserImageCommand(string userId, IFormFile file) : IRequest<UserImageResponse>;

public class UploadUserImageHandler(
    IUserService userService,
    IBlobStorageRepository repository) : IRequestHandler<UploadUserImageCommand, UserImageResponse>
{
    public async Task<UserImageResponse> Handle(UploadUserImageCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(request.userId, cancellationToken);
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
        var fileName = $"{Guid.NewGuid()}.{extension}";

        using var stream = request.file.OpenReadStream();
        user.AvatarUrl = await repository.UploadAsync(stream, fileName, request.file.ContentType, "media");

        await userService.UpdateAsync(user);

        return new UserImageResponse
        {
            AvatarUrl = user.AvatarUrl,
        };
    }
}