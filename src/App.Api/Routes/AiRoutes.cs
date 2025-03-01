using App.Application.Services;
using App.RestContracts.AI;

namespace App.Api.Routes;

public static class AiRoutes
{
    public static void MapAiRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/open-ai/test")
            .WithName("OpenAI")
            .WithTags("OpenAI");

        group.MapPost("/", async (SendMessageRequest request, IOpenAIService service) =>
            {
                return await service.GetChatCompletionAsync(request.Message);
            })
            .WithName("SendMessageToAI");
    }

}
