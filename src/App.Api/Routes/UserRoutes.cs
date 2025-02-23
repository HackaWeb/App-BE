using App.Application.Handlers.Users;
using App.RestContracts.User;
using App.RestContracts.Users.Requests;
using MediatR;
using System.Security.Claims;

namespace App.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/users").WithName("Users");

        group.MapGet("/me", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send(new GetUserByIdCommand(userId));
        }).WithName("GetCurrentUser").RequireAuthorization();


        group.MapPut("/me", async (HttpContext httpContext, IMediator mediator, UpdateUserRequest request) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var command = new UpdateUserCommand(userId, request.FirstName, request.LastName, request.Email, request.Username);
            
            return await mediator.Send(command);
        }).WithName("UpdateCurrentUser").RequireAuthorization();


        group.MapDelete("/me", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send(new DeleteUserCommand(userId));
        }).WithName("DeleteCurrentUser").RequireAuthorization();


        group.MapPost("/me/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send();

        }).WithName("UploadUserImage").RequireAuthorization();


        group.MapDelete("/me/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await mediator.Send();

        }).WithName("DeleteUserImage").RequireAuthorization();


        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            return await mediator.Send(new GetUserByIdCommand(id.ToString()));
        }).WithName("GetUserById");

    }
}