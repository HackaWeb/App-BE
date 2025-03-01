using App.Application;
using App.Application.Handlers;
using App.Application.Handlers.Trello;
using App.RestContracts.AI;
using MediatR;

namespace App.Api.Routes;

public static class AiRoutes
{
    public static void MapAiRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/open-ai/test")
            .WithName("OpenAI")
            .WithTags("OpenAI");

        group.MapPost("/", async (SendMessageRequest request, IMediator mediator) =>
            {
                var commandType = await mediator.Send(new IdentityCommand(request.Message));

                string botResponse;
                switch (commandType)
                {
                    case PromptCommands.AddCards:
                        botResponse = await mediator.Send(new SetupTrelloCardsCommand(request.Message));
                        break;
                    case PromptCommands.CreateBord:
                        botResponse = await mediator.Send(new SetupTrelloBoardCommand(request.Message));
                        break;
                    default:
                        botResponse = commandType.ToString();
                        break;
                }

                return botResponse;
            })
            .WithName("SendMessageToAI");
    }

}
