using App.Application.Cache;
using App.Application.Helpers;
using App.Application.Services;
using App.Domain;
using App.Domain.Exceptions;
using App.Domain.Settings;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace App.Application.Handlers.Trello
{
    public record SetupTrelloBoardCommand(string UserRequest) : IRequest<string>;

    public class SetupTrelloBoardHandler(
        IOpenAIService openAiService,
        IOptions<TrelloSettings> trelloOptions,
        HttpClient httpClient) : IRequestHandler<SetupTrelloBoardCommand, string>
    {
        public async Task<string> Handle(SetupTrelloBoardCommand boardCommand, CancellationToken cancellationToken)
        {
            var prompt = 
                $@"
                    You are an AI assistant for integrating with Trello. Please review the documentation at: https://developer.atlassian.com/cloud/trello/rest/.

                    Based on the following request, generate a valid JSON array of curl commands. Each boardCommand in the array should be a string representing a curl boardCommand that includes:

                    The HTTP method (POST, PUT, DELETE, or GET)
                    The URL for the Trello API request
                    Any required headers (if needed)
                    The request parameters (either as a request body or as query string parameters)
                    Important: Even if the API key and token are not provided in the user's request, you must always include the placeholders ""YOUR_TRELLO_API_KEY"" and ""YOUR_TRELLO_TOKEN"" in the generated commands. If a boardCommand depends on a boardId (or a similar value), use the placeholder ""YOUR_BOARD_ID"". Do not output any error message regarding missing API key, token, or boardId.

                    Please ensure that your response is in the same language as the user's request.

                    For example, an acceptable response might be:

                    [ ""curl -X POST ""https://api.trello.com/1/boards/"" -H ""Content-Keys: application/x-www-form-urlencoded"" -d ""name=My Board&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN"""", ""curl -X POST ""https://api.trello.com/1/boards/YOUR_BOARD_ID/labels/"" -H ""Content-Keys: application/x-www-form-urlencoded"" -d ""name=Project A&color=blue&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN"""", ""curl -X POST ""https://api.trello.com/1/lists/"" -H ""Content-Keys: application/x-www-form-urlencoded"" -d ""name=To Do&idBoard=YOUR_BOARD_ID&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN"""", ""curl -X POST ""https://api.trello.com/1/lists/"" -H ""Content-Keys: application/x-www-form-urlencoded"" -d ""name=In Progress&idBoard=YOUR_BOARD_ID&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN"""", ""curl -X POST ""https://api.trello.com/1/lists/"" -H ""Content-Keys: application/x-www-form-urlencoded"" -d ""name=Done&idBoard=YOUR_BOARD_ID&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN"""" ]

                    Now, process the following request and return only the JSON array of curl commands or an error message: ""{boardCommand.UserRequest}""
                ";

            string response = await openAiService.GetChatCompletionAsync(prompt);
            string openAiResponse = response.Replace("```json", "").Replace("```", "").Trim();

            string[] curlCommands;
            try
            {
                curlCommands = JsonConvert.DeserializeObject<string[]>(openAiResponse);
                if (curlCommands == null || curlCommands.Length == 0)
                {
                    throw new DomainException($"The OpenAI response cannot be parsed as a valid JSON array of curl commands. Response: {openAiResponse}",
                        (int)HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                throw new DomainException(openAiResponse, (int)HttpStatusCode.InternalServerError);
            }

            string boardId = null;

            foreach (var curlCommand in curlCommands)
            {
                string processedCommand = curlCommand;
                if (!string.IsNullOrEmpty(boardId))
                {
                    BoardCache.AddOrUpdate("My Board", boardId);
                    processedCommand = processedCommand.Replace("YOUR_BOARD_ID", boardId);
                }

                var apiRequest = ApplicationHelper.ParseCurlCommand(processedCommand);

                if (apiRequest.Parameters != null)
                {
                    if (apiRequest.Parameters.TryGetValue("key", out var keyVal))
                    {
                        apiRequest.Parameters["key"] = Environment.GetEnvironmentVariable(AppConstants.TRELLO_API_KEY)
                                                       ?? trelloOptions.Value.TrelloApiKey;
                    }
                    if (apiRequest.Parameters.TryGetValue("token", out var tokenVal))
                    {
                        apiRequest.Parameters["token"] = Environment.GetEnvironmentVariable(AppConstants.TRELLO_SECRET_KEY)
                                                         ?? trelloOptions.Value.TrelloSecret;
                    }
                }

                string result = await ApplicationHelper.ExecuteTrelloApiRequestAsync(apiRequest, httpClient);

                if (string.IsNullOrEmpty(boardId))
                {
                    try
                    {
                        dynamic json = JsonConvert.DeserializeObject(result);
                        boardId = json.id;
                    }
                    catch
                    {
                    }
                }
            }

            return "All done! Enjoy it!";
        }
    }
}
