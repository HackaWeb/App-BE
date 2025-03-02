using App.Application.Services;
using MediatR;

namespace App.Application.Handlers
{
    public record ConfirmUserCommand(string userInput) : IRequest<string>;
    class ConfirmationHandler(IOpenAIService openAiService) : IRequestHandler<ConfirmUserCommand, string>
    {
        public async Task<string> Handle(ConfirmUserCommand request, CancellationToken cancellationToken)
        {
            var confirmationPrompt = $@"
                You are an AI assistant that provides short confirmation messages for the user after a successful creation of something in their third-party service (e.g., Trello or Slack).

                Based on the following context, generate a single-sentence confirmation message. This message must be concise, positive, and must not contain any extra details or placeholders. Return only the message text, nothing else.

                Context:
                ""{request.userInput}""

                Example:
                If context says 'Trello board “Team Tasks” and 3 lists created', a valid response might be 'Your Trello board “Team Tasks” with 3 lists has been successfully created!'

                Now, provide your confirmation message based on the context:
                ""{request.userInput}""
                ";


            return await openAiService.GetChatCompletionAsync(request.userInput);
        }
    }
}
