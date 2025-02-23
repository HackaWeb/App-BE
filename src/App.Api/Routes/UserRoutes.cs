using App.Application.Handlers.Users;
using App.RestContracts.Users;
using App.RestContracts.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace App.Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/users").WithName("Users").WithTags("Users");

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
            await mediator.Send(new DeleteUserCommand(userId));

            return Results.Ok();
        }).WithName("DeleteCurrentUser").RequireAuthorization();


        group.MapPost("/me/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!httpContext.Request.HasFormContentType || httpContext.Request.Form.Files.Count == 0)
            {
                return Results.BadRequest("No file uploaded");
            }

            var file = httpContext.Request.Form.Files[0];

            return Results.Ok(await mediator.Send(new UploadUserImageCommand(userId, file)));
        }).WithName("UploadUserImage").RequireAuthorization();


        group.MapDelete("/me/image", async (HttpContext httpContext, IMediator mediator) =>
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await mediator.Send(new DeleteUserImageCommand(userId));

            return Results.Ok();
        }).WithName("DeleteUserImage").RequireAuthorization();
        
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            return await mediator.Send(new GetUserByIdCommand(id.ToString()));
        }).WithName("GetUserById");
    }
}