using App.Application.Handlers.Users;
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

        group.MapGet("/{id:guid}", async (HttpContext httpContext, Guid id, IMediator mediator) =>
        {
            return await mediator.Send(new GetUserByIdCommand(id.ToString()));
        })
            .WithName("GetUserById");
    }
}
