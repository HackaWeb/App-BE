using App.Application.Handlers.Users;
using App.RestContracts.Users.Requests;
using MediatR;
using System.Security.Claims;

namespace App.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/users")
            .WithName("Users")
            .WithTags("Users");

        group.MapGet("/me", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send(new GetUserByIdCommand(userId));
        })
            .WithName("GetCurrentUser")
            .RequireAuthorization();


        group.MapPut("/me", async (HttpContext httpContext, IMediator mediator, UpdateUserRequest request) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var command = new UpdateUserCommand(userId, request.FirstName, request.LastName, request.Email, request.Username);
            
            return await mediator.Send(command);
        })
            .WithName("UpdateCurrentUser")
            .RequireAuthorization();


        group.MapDelete("/me", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await mediator.Send(new DeleteUserCommand(userId));

            return Results.Ok();
        })
            .WithName("DeleteCurrentUser")
            .RequireAuthorization();


        group.MapPost("/me/image", async (HttpContext httpContext, IFormFile file, IMediator mediator) =>
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
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .DisableAntiforgery();


        group.MapDelete("/me/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await mediator.Send(new DeleteUserImageCommand(userId));

            return Results.Ok();
        })
            .WithName("DeleteUserImage")
            .RequireAuthorization();
    }
}