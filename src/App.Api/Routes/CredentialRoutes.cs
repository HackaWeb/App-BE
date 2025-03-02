using App.Application.Handlers.Transactions;
using App.Application.Handlers.Users;
using App.Domain.Enums;
using App.RestContracts.Users.Requests;
using MediatR;
using System.Security.Claims;

namespace App.Api.Routes;

public static class CredentialRoutes
{
    public static void MapCredentialRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/credentials")
            .WithName("Credentials")
            .WithTags("Credentials");

        group.MapPost("/", async (CreateCredentialRequest request, HttpContext httpContext, IMediator mediator) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await mediator.Send(new AddUserCredentialsCommand(request.KeyType, request.Value, Guid.Parse(userId)));
            })
            .WithName("AddCredentials")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext httpContext, IMediator mediator) =>
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await mediator.Send(new GetUserCredentialsCommand(Guid.Parse(userId)));
            })
            .WithName("GetCredentials")
            .RequireAuthorization();

    }
}
