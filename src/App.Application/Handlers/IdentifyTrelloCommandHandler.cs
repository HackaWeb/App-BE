using App.Application.Services;
using App.Domain.Exceptions;
using MediatR;
using System.Net;

namespace App.Application.Handlers
{
    public record IdentityTrelloCommand(string userMessage) : IRequest<PromptCommands>;

    public class IdentifyTrelloCommandHandler(IOpenAIService openAiService) : IRequestHandler<IdentityTrelloCommand, PromptCommands>
    {
        public async Task<PromptCommands> Handle(IdentityTrelloCommand request, CancellationToken cancellationToken)
        {
            var classificationPrompt = $@"
                You are an AI assistant that classifies user requests for Trello integration. Based on the following user request, return only one word indicating the action type. Use one of the following values:
                - ""{nameof(PromptCommands.CreateBord)}"" (if the request is about creating a board)
                - ""{nameof(PromptCommands.AddCards)}"" (if the request is about adding a card)

                Respond in the same language as the user's request.

                Process the following request and return only the action type:
                ""{request.userMessage}""
                
            ";


            string response = await openAiService.GetChatCompletionAsync(classificationPrompt);

            if (!Enum.TryParse<PromptCommands>(response, out var prompt))
            {
                throw new DomainException(response, (int)HttpStatusCode.OK);
            }

            return prompt;
        }
    }
}
