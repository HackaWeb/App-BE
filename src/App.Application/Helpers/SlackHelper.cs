namespace App.Application.Helpers;

using App.Application.Handlers.Slack;
using System.Text.RegularExpressions;

public static class SlackHelper
{
    public static SlackApiRequest ParseCurlCommand(string curlCommand)
    {
        if (string.IsNullOrWhiteSpace(curlCommand) || !curlCommand.StartsWith("curl", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("The provided response is not a valid curl command.");
        }

        var request = new SlackApiRequest();

        var methodMatch = Regex.Match(curlCommand, @"-X\s+(\w+)", RegexOptions.IgnoreCase);
        request.HttpMethod = methodMatch.Success
            ? methodMatch.Groups[1].Value.ToUpper()
            : "GET";

        var urlMatch = Regex.Match(curlCommand, @"-X\s+\w+\s+""([^""]+)""", RegexOptions.IgnoreCase);
        if (!urlMatch.Success)
        {
            urlMatch = Regex.Match(curlCommand, @"curl\s+""([^""]+)""", RegexOptions.IgnoreCase);
        }
        if (urlMatch.Success)
        {
            request.Url = urlMatch.Groups[1].Value.Trim();
        }
        else
        {
            throw new Exception("URL not found in the curl command.");
        }

        var headerMatches = Regex.Matches(curlCommand, @"-H\s+""([^:""]+):\s*([^""]+)""", RegexOptions.IgnoreCase);
        foreach (Match match in headerMatches)
        {
            if (match.Success)
            {
                var headerName = match.Groups[1].Value.Trim();
                var headerValue = match.Groups[2].Value.Trim();
                request.Headers[headerName] = headerValue;
            }
        }

        var dataRegex = new Regex(
            @"(--data(?:-binary)?)\s+(['""])(?<content>.*?)\2",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );

        var dataMatch = dataRegex.Match(curlCommand);
        if (dataMatch.Success)
        {
            request.Body = dataMatch.Groups["content"].Value.Trim();
        }

        return request;
    }

    public static async Task<string> ExecuteSlackRequestAsync(SlackApiRequest apiRequest, HttpClient httpClient)
    {
        using var httpRequest = new HttpRequestMessage(new HttpMethod(apiRequest.HttpMethod), apiRequest.Url);

        foreach (var kv in apiRequest.Headers)
        {
            if (kv.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                httpRequest.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", kv.Value.Replace("Bearer ", ""));
            }
            else
            {
                httpRequest.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
        }

        if ((apiRequest.HttpMethod == "POST" || apiRequest.HttpMethod == "PUT") && !string.IsNullOrEmpty(apiRequest.Body))
        {
            httpRequest.Content = new StringContent(apiRequest.Body, System.Text.Encoding.UTF8, "application/json");
        }

        var response = await httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Slack API returned error. Status Code: {response.StatusCode}, Body: {responseContent}");
        }

        return responseContent;
    }
}