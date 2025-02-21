using App.Application.Handlers.Auth;
using App.Domain.Exceptions;
using App.RestContracts.Auth.Requests;
using MediatR;
using System.Net;

namespace App.Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth");

        group.MapPost("/register", async (IMediator mediator, RegisterUserRequest request) =>
        {
            var command = new RegisterUserCommand();
            await mediator.Send(command);
        }).WithName("RegisterUser");

        group.MapPost("/login", async (IMediator mediator, LoginUserRequest request) =>
        {
            var command = new LoginUserCommand();
            await mediator.Send(command);
        }).WithName("RegisterUser");

        group.MapPost("/refresh/{refreshToken}", async (IMediator mediator, string refreshToken) =>
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new DomainException("refresh token can't be empty!", (int)HttpStatusCode.BadRequest);
            }

            var command = new RefreshTokenCommand();
            await mediator.Send(command);
        }).WithName("RefreshToken");
    }
}
