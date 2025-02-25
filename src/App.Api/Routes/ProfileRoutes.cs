using App.Application.Handlers.Users;
using App.RestContracts.Shared;
using App.RestContracts.Users.Requests;
using App.RestContracts.Users.Responses;
using MediatR;
using System.Security.Claims;

namespace App.Api.Routes;

public static class ProfileRoutes
{
    public static void MapProfileRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/profile")
            .WithName("Profile")
            .WithTags("Profile");

        group.MapGet("/", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send(new GetUserByIdCommand(userId));
        })
            .WithName("GetCurrentUser")
            .RequireAuthorization();


        group.MapPut("/", async (HttpContext httpContext, IMediator mediator, UpdateUserRequest request) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var command = new UpdateUserCommand(userId, request.FirstName, request.LastName, request.Email);
            
            return await mediator.Send(command);
        })
            .WithName("UpdateCurrentUser")
            .RequireAuthorization();


        group.MapDelete("/", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await mediator.Send(new DeleteUserCommand(userId));

            return new ResultResponse
            {
                IsSuccess = true,
            };
        })
            .WithName("DeleteCurrentUser")
            .RequireAuthorization();


        group.MapPost("/image", async (HttpContext httpContext, IFormFile file, IMediator mediator) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest("No file uploaded");
                }

                return Results.Ok(await mediator.Send(new UploadUserImageCommand(userId, file)));
            })
            .WithName("UploadUserImage")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UserImageResponse>(StatusCodes.Status200OK)
            .Produces<ResultResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .DisableAntiforgery();


        group.MapDelete("/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await mediator.Send(new DeleteUserImageCommand(userId));

            return new ResultResponse
            {
                IsSuccess = true,
            };
        })
            .WithName("DeleteUserImage")
            .RequireAuthorization();
    }
}