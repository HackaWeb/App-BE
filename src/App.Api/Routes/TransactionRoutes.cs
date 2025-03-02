using App.Application.Handlers.Tags;
using App.Application.Handlers.Transactions;
using App.Domain.Enums;
using App.RestContracts.Tags;
using App.RestContracts.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Security.Claims;

namespace App.Api.Routes;

public static class TransactionRoutes
{
    public static void MapTransactiongRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/transactions")
            .WithName("Transactions")
            .WithTags("Transactions");

        group.MapGet("/", async (HttpContext httpContext, IMediator mediator) =>
        {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await mediator.Send(new GetUserTransactionsCommand(Guid.Parse(userId)));
            })
            .WithName("GetUserTransactions")
            .RequireAuthorization();

        group.MapGet("/{userId:guid}", async (Guid userId, IMediator mediator) =>
            {
                return await mediator.Send(new GetUserTransactionsCommand(userId));
            })
            .WithName("GetTransactionsByUserIdByAdmin")
            .RequireAuthorization("Admin");

        group.MapGet("/all", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetAllTransactionsCommand());
            })
            .WithName("GetAllTransactions")
            .RequireAuthorization("Admin");

        group.MapPost("/", async (CreateTransactionRequest request, HttpContext httpContext, IMediator mediator) =>
        {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return await mediator.Send(new AddTransactionCommand(request.CreatedAt, request.Amount, Guid.Parse(userId), TransactionType.Deposit));
            })
            .WithName("DepositMoney")
            .RequireAuthorization();

    }
}
