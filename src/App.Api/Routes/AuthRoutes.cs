using App.Application.Handlers.Auth;
using App.Domain.Exceptions;
using App.RestContracts.Auth.Requests;
using Azure.Core;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Net;

namespace App.Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/auth")
            .WithName("Auth")
            .WithTags("Authentication");

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

        group.MapGet("/google", async (IMediator mediator, string token) =>
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token);
            var email = payload.Email;
            var firstName = payload.GivenName;
            var lastName = payload.FamilyName;

            return await mediator.Send(new ThirdPartyAuthCommand(email, firstName, lastName));
        }).WithName("GoogleLogin");

        //group.MapGet("/login/github", async (HttpContext context) =>
        //{
        //    var authProperties = new AuthenticationProperties { RedirectUri = "api/auth/github-callback" };
        //    return Results.Challenge(authProperties, new[] { "GitHub" });
        //}).WithName("GithubLogin");

        //group.MapGet("/github-callback", async (IMediator mediator) =>
        //{
        //        var response = await mediator.Send(new ThirdPartyAuthCommand("GitHub"));
        //    return response;
        //}).WithName("GitHubCallback");
    }
}
