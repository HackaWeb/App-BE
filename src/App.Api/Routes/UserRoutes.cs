using App.Application.Handlers.Users;
using App.RestContracts.Shared;
using App.RestContracts.Users.Requests;
using App.RestContracts.Users.Responses;
using MediatR;

namespace App.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/users")
            .WithName("Users")
            .WithTags("Users");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            return await mediator.Send(new GetUserByIdCommand(id.ToString()));
        })
            .WithName("GetUserById");

        group.MapPut("/{id}", async (UpdateUserRequest request, string id, IMediator mediator) =>
            {
                return await mediator.Send(
                    new UpdateUserCommand(id, request.FirstName, request.LastName, request.Email));
            })
            .WithName("UpdateUserByAdmin")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetAllUsersCommand());
            })
            .WithName("GetAllUsers");
            //.RequireAuthorization("Admin");

        group.MapPost("/{userId}/image", async (string userId, IFormFile file, IMediator mediator) =>
            { 
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest("No file uploaded");
                }

                return Results.Ok(await mediator.Send(new UploadUserImageCommand(userId, file)));
            })
            .WithName("UploadUserImageByAdmin")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UserImageResponse>(StatusCodes.Status200OK)
            .Produces<ResultResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Admin")
            .DisableAntiforgery();


        group.MapDelete("/{userId}/image", async (string userId, IMediator mediator) =>
            {
                await mediator.Send(new DeleteUserImageCommand(userId));

                return new ResultResponse
                {
                    IsSuccess = true,
                };
            })
            .WithName("DeleteUserImageByAdmin")
            .RequireAuthorization("Admin");

        group.MapDelete("/{userId}", async (string userId, IMediator mediator) =>
            {
                await mediator.Send(new DeleteUserCommand(userId));

                return new ResultResponse
                {
                    IsSuccess = true,
                };
            })
            .WithName("DeleteUserByAdmin")
            .RequireAuthorization("Admin");
    }
}
