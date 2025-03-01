using App.Application.Services;
using App.Domain;
using App.Domain.Exceptions;
using MediatR;
using System.Net;
using System.Text.RegularExpressions;

namespace App.Application.Handlers
{
    public record SendToTrelloCommand(string UserRequest) : IRequest<string>;

    public class TrelloHandler(
        IOpenAIService openAiService,
        HttpClient httpClient) : IRequestHandler<SendToTrelloCommand, string>
    {
        public async Task<string> Handle(SendToTrelloCommand command, CancellationToken cancellationToken)
        {
            string prompt = $@"
                You are an AI assistant for integrating with Trello. Please review the documentation at:  
                https://developer.atlassian.com/cloud/trello/rest/

                Based on the user’s request:
                1. If the request is **valid** and can be fulfilled by Trello’s API, **respond only** with the following curl command (or an array of curl commands) **in exactly this format** (do not add any extra text or formatting before or after):

                curl -X POST ""https://api.trello.com/1/boards/"" -H ""Content-Type: application/x-www-form-urlencoded"" -d ""name=BOARD-NAME&key=KEY&token=TOKEN""

                Replace `name=back` (or any other parameter) as needed, but always include `key=KEY` and `token=TOKEN` exactly in uppercase. Ignore the absence of real token or key in the user’s request and **always** put the placeholders `KEY` and `TOKEN`.

                2. If the request **cannot** be executed for **any** reason (e.g., invalid parameters, missing information, or something else preventing a valid Trello API call), then respond with **only** an error message WITH EXPLANATION. The error message must be in the **same language** the user used in their request, and contain **no additional text** beyond the error explanation.

                API KEY: 478
                TOKEN: ATTA

                Process the following user request and respond accordingly:
                ""{command.UserRequest}""
                ";

            string openAiResponse = await openAiService.GetChatCompletionAsync(prompt);
            TrelloApiRequest apiRequest;

            try
            {
                apiRequest = ParseCurlCommand(openAiResponse);
                if (apiRequest is null)
                {
                    throw new DomainException($"The OpenAI response cannot be parsed as a valid curl command. {openAiResponse}", (int)HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                return openAiResponse;
            }


            if (apiRequest.Parameters != null)
            {
                if (apiRequest.Parameters.TryGetValue("key", out var keyVal))
                {
                    apiRequest.Parameters["key"] = Environment.GetEnvironmentVariable(AppConstants.TRELLO_API_KEY) 
                                                   ?? throw new DomainException("API KEY ABSENT", (int)HttpStatusCode.BadRequest);
                }
                if (apiRequest.Parameters.TryGetValue("token", out var tokenVal))
                {
                    apiRequest.Parameters["token"] = Environment.GetEnvironmentVariable(AppConstants.TRELLO_SECRET_KEY) 
                                                     ?? throw new DomainException("TOKEN ABSENT", (int)HttpStatusCode.BadRequest);
                }
            }

            string result = await ExecuteTrelloApiRequestAsync(apiRequest, httpClient);
            return openAiResponse;
        }

        private async Task<string> ExecuteTrelloApiRequestAsync(TrelloApiRequest apiRequest, HttpClient httpClient)
        {
            var content = new FormUrlEncodedContent(apiRequest.Parameters);
            HttpResponseMessage response;

            switch (apiRequest.Method.ToUpperInvariant())
            {
                case "POST":
                    response = await httpClient.PostAsync(apiRequest.Url, content);
                    break;
                case "PUT":
                    response = await httpClient.PutAsync(apiRequest.Url, content);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(apiRequest.Url);
                    break;
                case "GET":
                    var query = await content.ReadAsStringAsync();
                    response = await httpClient.GetAsync($"{apiRequest.Url}?{query}");
                    break;
                default:
                    throw new DomainException("Unknown HTTP method.", (int)HttpStatusCode.InternalServerError);
            }

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Trello API error: {errorContent}");
            }
        }

        private TrelloApiRequest ParseCurlCommand(string curlCommand)
        {
            if (string.IsNullOrWhiteSpace(curlCommand) || !curlCommand.StartsWith("curl", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The provided response is not a valid curl command.");
            }

            var request = new TrelloApiRequest
            {
                Headers = new Dictionary<string, string>(),
                Parameters = new Dictionary<string, string>()
            };

            var methodMatch = Regex.Match(curlCommand, @"-X\s+(\w+)");
            request.Method = methodMatch.Success ? methodMatch.Groups[1].Value.ToUpper() : "GET";

            var urlMatch = Regex.Match(curlCommand, @"-X\s+\w+\s+""([^""]+)""");
            if (!urlMatch.Success)
            {
                urlMatch = Regex.Match(curlCommand, @"curl\s+""([^""]+)""");
            }
            if (urlMatch.Success)
            {
                request.Url = urlMatch.Groups[1].Value;
            }
            else
            {
                throw new DomainException("URL not found in the curl command.");
            }

            var headerMatches = Regex.Matches(curlCommand, @"-H\s+""([^:""]+):\s*([^""]+)""");
            foreach (Match match in headerMatches)
            {
                if (match.Success)
                {
                    request.Headers[match.Groups[1].Value.Trim()] = match.Groups[2].Value.Trim();
                }
            }

            var dataMatch = Regex.Match(curlCommand, @"-d\s+""([^""]+)""");
            if (dataMatch.Success)
            {
                var dataString = dataMatch.Groups[1].Value;
                var pairs = dataString.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length == 2)
                    {
                        request.Parameters[WebUtility.UrlDecode(kv[0])] = WebUtility.UrlDecode(kv[1]);
                    }
                }
            }

            return request;
        }
    }
}

public class TrelloApiRequest
{
    public string Url { get; set; }
    public string Method { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
    public Dictionary<string, string> Headers { get; set; }
}

