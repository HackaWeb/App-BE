using App.Application.Services;
using App.Domain.Enums;
using MediatR;

namespace App.Application.Handlers
{
    public record GetThirdPartyServiceCommand(string userInput) : IRequest<ThirdPartyService>;

    public class GetThirdPartyServiceHandler(IOpenAIService openAiService) : IRequestHandler<GetThirdPartyServiceCommand, ThirdPartyService>
    {
        public async Task<ThirdPartyService> Handle(GetThirdPartyServiceCommand request, CancellationToken cancellationToken)
        {
            var prompt = $@"
                You are a classification assistant. The user says: ""{request.userInput}""

                You must classify whether the user's request refers to 'Trello' or 'Slack'. 

                Important:
                - Return exactly one of the following strings: 'Trello' or 'Slack'.
                - Return no extra text, punctuation, or explanation. Just the single word.

                If you cannot decide or if the user's request is too ambiguous, default to 'Slack'.
                ";

            var response = await openAiService.GetChatCompletionAsync(prompt);
            var classification = response.Trim();


            if (classification.Equals("Trello", StringComparison.OrdinalIgnoreCase))
            {
                return ThirdPartyService.Trello;
            }
            else if (classification.Equals("Slack", StringComparison.OrdinalIgnoreCase))
            {
                return ThirdPartyService.Slack;
            }
            else
            {
                return ThirdPartyService.Slack;
            }
        }
    }
}
