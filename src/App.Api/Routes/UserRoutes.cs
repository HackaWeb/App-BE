using App.Application.Handlers.Users;
using App.RestContracts.Users.Requests;
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
            .WithName("UpdateUser")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetAllUsersCommand());
            })
            .WithName("GetAllUsers")
            .RequireAuthorization("Admin");
    }
}
