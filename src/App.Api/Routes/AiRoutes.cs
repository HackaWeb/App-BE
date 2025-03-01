using App.Application.Handlers;
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
                return await mediator.Send(new SendToTrelloCommand(request.Message));
            })
            .WithName("SendMessageToAI");
    }

}
