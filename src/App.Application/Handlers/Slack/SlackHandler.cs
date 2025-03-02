using App.Application.Helpers;
using App.Application.Services;
using App.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace App.Application.Handlers.Slack
{
    public record SetupSlackCommand(string UserRequest, string? slackToken) : IRequest<string>;

    public class SetupSlackHandler(
        IOpenAIService openAiService,
        IOptions<SlackSettings> slackOptions,
        HttpClient httpClient) : IRequestHandler<SetupSlackCommand, string>
    {
        public async Task<string> Handle(SetupSlackCommand command, CancellationToken cancellationToken)
        {
            var slackToken = Environment.GetEnvironmentVariable("SLACK_TOKEN");

            var prompt = $@"
                You are an AI assistant for integrating with Slack. Please review the Slack API documentation at: https://api.slack.com/methods.

                Based on the following request, generate a valid JSON array of curl commands. Each command in the array should be a string representing a curl command that includes:
                - An HTTP method (POST, GET, etc.)
                - The URL for the Slack API endpoint
                - All required headers (Authorization with Bearer token, Content-KeyType)
                - The request payload or query string parameters

                Important:
                - You must always include the placeholder ""YOUR_SLACK_TOKEN"" for the token.
                - Do not output any error messages about missing tokens or placeholders.
                - Please respond in the same language as the user's request.

                Example Slack commands:
                1) Set user status:
                curl -X POST ""https://slack.com/api/users.profile.set"" \
                  -H ""Authorization: Bearer YOUR_SLACK_TOKEN"" \
                  -H ""Content-KeyType: application/json"" \
                  --data '{{ ""profile"": {{ ""status_text"": ""Working on Slack API"", ""status_emoji"": "":computer:"" }} }}'

                2) Create a channel:
                curl -X POST ""https://slack.com/api/conversations.create"" \
                  -H ""Authorization: Bearer YOUR_SLACK_TOKEN"" \
                  -H ""Content-KeyType: application/json"" \
                  --data '{{ ""name"": ""new-channel"", ""is_private"": false }}'

                3) Create a user group:
                curl -X POST ""https://slack.com/api/usergroups.create"" \
                  -H ""Authorization: Bearer YOUR_SLACK_TOKEN"" \
                  -H ""Content-KeyType: application/json"" \
                  --data '{{ ""name"": ""developers"", ""handle"": ""devs"" }}'

                4) Add a reminder:
                curl -X POST ""https://slack.com/api/reminders.add"" \
                  -H ""Authorization: Bearer YOUR_SLACK_TOKEN"" \
                  -H ""Content-KeyType: application/json"" \
                  --data '{{ ""text"": ""Stand-up meeting in 10 minutes"", ""time"": ""in 10 minutes""}}'

                Now, process the following request and return only the JSON array of curl commands or an error message:
                ""{command.UserRequest}""
                ";

            var response = await openAiService.GetChatCompletionAsync(prompt);
            var openAiResponse = response
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            string[] curlCommands;
            try
            {
                curlCommands = JsonConvert.DeserializeObject<string[]>(openAiResponse);
                if (curlCommands == null || curlCommands.Length == 0)
                {
                    throw new DomainException(
                        $"The OpenAI response cannot be parsed as a valid JSON array of curl commands. Response: {openAiResponse}",
                        (int)HttpStatusCode.InternalServerError
                    );
                }
            }
            catch (Exception)
            {
                throw new DomainException(openAiResponse, (int)HttpStatusCode.InternalServerError);
            }

            var aggregatedResponse = new StringBuilder();

            foreach (var curlCommand in curlCommands)
            {
                var apiRequest = SlackHelper.ParseCurlCommand(curlCommand);

                if (apiRequest.Headers.TryGetValue("Authorization", out var authValue))
                {
                    apiRequest.Headers["Authorization"] = authValue.Replace("YOUR_SLACK_TOKEN", command.slackToken ?? slackToken);
                }
                if (!string.IsNullOrEmpty(apiRequest.Body))
                {
                    apiRequest.Body = apiRequest.Body.Replace("YOUR_SLACK_TOKEN", command.slackToken ?? slackToken);
                }
                if (!string.IsNullOrEmpty(apiRequest.Url))
                {
                    apiRequest.Url = apiRequest.Url.Replace("YOUR_SLACK_TOKEN", command.slackToken ?? slackToken);
                }

                await SlackHelper.ExecuteSlackRequestAsync(apiRequest, httpClient);
                aggregatedResponse.AppendLine($"Executed command: {curlCommand}");
            }

            return aggregatedResponse.ToString().Trim();
        }
    }

    public class SlackSettings
    {
        public string SlackToken { get; set; }
    }

    public class SlackApiRequest
    {
        public string HttpMethod { get; set; }       
        public string Url { get; set; }               
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; }             
    }
}
