using App.Application.Helpers;
using App.Application.Services;
using App.Domain.Exceptions;
using MediatR;
using Newtonsoft.Json;
using System.Net;
using App.Domain;
using App.Domain.Settings;
using Microsoft.Extensions.Options;

namespace App.Application.Handlers.Trello;

public record SetupTrelloCardsCommand(string UserRequest, string trelloApiKey, string trelloToken) : IRequest<string>;

public class SetupTrelloCardsHandler(
    IOpenAIService openAiService,
    //IOptions<TrelloSettings> trelloOptions,
    HttpClient httpClient) : IRequestHandler<SetupTrelloCardsCommand, string>
{
    public async Task<string> Handle(SetupTrelloCardsCommand request, CancellationToken cancellationToken)
    {
        var prompt = $@"
            You are an AI assistant for integrating with Trello. Please review the documentation at: https://developer.atlassian.com/cloud/trello/rest/.
            Based on the following request, generate a valid JSON object with the following structure:
            {{
              ""boardTitle"": <the title of the board as extracted from the user's request>,
              ""listTitle"": <the title of the list as extracted from the user's request>,
              ""commands"": [
                <a curl command to retrieve all boards for the user (to find the board by name)>,
                <a curl command to retrieve all lists on the specified board (using the placeholder ""YOUR_BOARD_ID"")>,
                <a curl command to create a new card on the specified list (using the placeholder ""YOUR_LIST_ID"")>
              ]
            }}

            Each curl command must include:
            - HTTP method (POST, PUT, DELETE, or GET),
            - URL for the Trello API request,
            - Headers (if needed),
            - Request parameters (body or query string).

            Important:
            - Even if the API key and token are not provided in the user's request, you must always include the placeholders ""YOUR_TRELLO_API_KEY"" and ""YOUR_TRELLO_TOKEN"" in the generated commands.
            - If a command depends on a boardId, use the placeholder ""YOUR_BOARD_ID"".
            - If a command depends on a listId, use the placeholder ""YOUR_LIST_ID"".
            - Do not output any error message regarding missing API key, token, boardId, or listId.
            - Please respond in the same language as the user's request.

            Example response:
            {{
              ""boardTitle"": ""My Boards"",
              ""listTitle"": ""To Do"",
              ""commands"": [
                ""curl -X GET \""https://api.trello.com/1/members/me/boards?key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN\"""",
                ""curl -X GET \""https://api.trello.com/1/boards/YOUR_BOARD_ID/lists?key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN\"""",
                ""curl -X POST \""https://api.trello.com/1/cards\"" -H \""Content-Keys: application/x-www-form-urlencoded\"" -d \""name=Fix critical bug&desc=Please fix the bug ASAP&idList=YOUR_LIST_ID&key=YOUR_TRELLO_API_KEY&token=YOUR_TRELLO_TOKEN\""""
              ]
            }}

            Process the following request and return only the JSON object or an error message:
            ""{request.UserRequest}""
            ";


        string response = await openAiService.GetChatCompletionAsync(prompt);
        string openAiResponse = response.Replace("```json", "").Replace("```", "").Trim();

        var trelloResponseObject = JsonConvert.DeserializeObject<TrelloResponse>(openAiResponse);
        string boardTitle = trelloResponseObject.BoardTitle;
        string listTitle = trelloResponseObject.ListTitle;


        string[] curlCommands;

        try
        {
            curlCommands = trelloResponseObject.Commands;
            if (curlCommands == null || curlCommands.Length == 0)
            {
                throw new DomainException($"The OpenAI response cannot be parsed as a valid JSON array of curl commands. Response: {openAiResponse}", (int)HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception e)
        {
            throw new DomainException(e.Message, (int)HttpStatusCode.InternalServerError);
        }

        string boardId = null;
        string listId = null;
        string aggregatedResponse = string.Empty;

        foreach (var curlCommand in curlCommands)
        {
            string processedCommand = curlCommand;
            if (!string.IsNullOrEmpty(boardId) || !string.IsNullOrEmpty(listId))
            {
                processedCommand = processedCommand.Replace("YOUR_BOARD_ID", boardId);
                processedCommand = processedCommand.Replace("YOUR_LIST_ID", listId);
            }

            var apiRequest = ApplicationHelper.ParseCurlCommand(processedCommand);

            if (apiRequest.Parameters != null)
            {
                if (apiRequest.Parameters.TryGetValue("key", out var keyVal))
                {
                    apiRequest.Parameters["key"] = request.trelloApiKey
                                                    ?? Environment.GetEnvironmentVariable(AppConstants.TRELLO_API_KEY)!;
                }
                apiRequest.Url = apiRequest.Url.Replace("YOUR_TRELLO_API_KEY", Environment.GetEnvironmentVariable(AppConstants.TRELLO_API_KEY));
                if (apiRequest.Parameters.TryGetValue("token", out var tokenVal))
                {
                    apiRequest.Parameters["token"] = request.trelloToken
                                                        ?? Environment.GetEnvironmentVariable(AppConstants.TRELLO_SECRET_KEY)!;
                }
                apiRequest.Url = apiRequest.Url.Replace("YOUR_TRELLO_TOKEN", Environment.GetEnvironmentVariable(AppConstants.TRELLO_SECRET_KEY));
            }

            var result = await ApplicationHelper.ExecuteTrelloApiRequestAsync(apiRequest, httpClient);

            if (string.IsNullOrEmpty(boardId) && apiRequest.Method.ToUpper() == "GET" && apiRequest.Url.Contains("/boards"))
            {
                try
                {
                    dynamic boards = JsonConvert.DeserializeObject(result);
                    foreach (var board in boards)
                    {
                        if (board.name.ToString().Equals(boardTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            boardId = board.id;
                            break;
                        }
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(listId) && apiRequest.Method.ToUpper() == "GET" && apiRequest.Url.Contains("/lists"))
            {
                try
                {
                    dynamic lists = JsonConvert.DeserializeObject(result);
                    foreach (var list in lists)
                    {
                        if (list.name.ToString().Equals(listTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            listId = list.id;
                            break;
                        }
                    }
                }
                catch
                {
                }
            }

            aggregatedResponse += result + "\n";
        }

        return "All done! Enjoy it!";

    }

    public class TrelloResponse
    {
        public string BoardTitle { get; set; }
        public string ListTitle { get; set; }
        public string[] Commands { get; set; }
    }

}