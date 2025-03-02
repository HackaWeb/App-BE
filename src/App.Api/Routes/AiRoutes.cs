using App.Application;
using App.Application.Handlers;
using App.Application.Handlers.Slack;
using App.Application.Handlers.Trello;
using App.Domain.Enums;
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

        //group.MapPost("/", async (SendMessageRequest request, IMediator mediator) =>
        //    {
        //        var thirdPartyType = await mediator.Send(new GetThirdPartyServiceCommand(request.Message));

        //        if (thirdPartyType == ThirdPartyService.Slack)
        //        {
        //            await mediator.Send(new SetupSlackCommand(request.Message));
        //            return;
        //        }

        //        if (thirdPartyType == ThirdPartyService.Trello)
        //        {
        //            var commandType = await mediator.Send(new IdentityTrelloCommand(request.Message));

        //            var t = commandType switch
        //            {
        //                PromptCommands.AddCards => await mediator.Send(new SetupTrelloCardsCommand(request.Message)),
        //                PromptCommands.CreateBord => await mediator.Send(new SetupTrelloBoardCommand(request.Message)),
        //                _ => commandType.ToString()
        //            };
        //        }
        //        //return botResponse;
        //    })
        //    .WithName("SendMessageToAI");
    }

}
