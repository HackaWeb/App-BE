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
        var group = app.MapGroup("api/auth").WithName("Auth");

        group.MapPost("/register", async (IMediator mediator, RegisterUserRequest request) =>
        {
            var command = new RegisterUserCommand(request.Email, request.UserName, request.Password);
            return await mediator.Send(command);
        }).WithName("RegisterUser");

        group.MapPost("/login", async (IMediator mediator, LoginUserRequest request) =>
        {
            var command = new LoginUserCommand(request.Username, request.Password);
            return await mediator.Send(command);
        }).WithName("LoginUser");

        group.MapPost("/refresh", async (IMediator mediator, RefreshTokenRequest request) =>
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new DomainException("refresh token can't be empty!", (int)HttpStatusCode.BadRequest);
            }

            var command = new RefreshTokenCommand(request.RefreshToken);
            return await mediator.Send(command);
        }).WithName("RefreshToken");
    }
}
