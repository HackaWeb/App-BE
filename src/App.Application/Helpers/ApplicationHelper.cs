using App.Domain.Exceptions;
using System.Net;
using System.Text.RegularExpressions;

namespace App.Application.Helpers;

public static class ApplicationHelper
{
    public static async Task<string> ExecuteTrelloApiRequestAsync(TrelloApiRequest apiRequest, HttpClient httpClient)
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
                var requestUrl = string.IsNullOrWhiteSpace(query) ? apiRequest.Url : $"{apiRequest.Url}?{query}";
                response = await httpClient.GetAsync(requestUrl);
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

    public static TrelloApiRequest ParseCurlCommand(string curlCommand)
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
            request.Url = urlMatch.Groups[1].Value.Trim('"');
        }
        else
        {
            throw new Exception("URL not found in the curl command.");
        }

        var headerMatches = Regex.Matches(curlCommand, @"-H\s+""([^:""]+):\s*([^""]+)""");
        foreach (Match match in headerMatches)
        {
            if (match.Success)
            {
                var headerName = match.Groups[1].Value.Trim().Trim('"');
                var headerValue = match.Groups[2].Value.Trim().Trim('"');
                request.Headers[headerName] = headerValue;
            }
        }

        var dataMatch = Regex.Match(curlCommand, @"-d\s+""([^""]+)""");
        if (dataMatch.Success)
        {
            var dataString = dataMatch.Groups[1].Value.Trim('"');
            var pairs = dataString.Split('&', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2)
                {
                    request.Parameters[WebUtility.UrlDecode(kv[0]).Trim('"')] = WebUtility.UrlDecode(kv[1]).Trim('"');
                }
            }
        }

        return request;
    }
}

public class TrelloApiRequest
{
    public string Url { get; set; }
    public string Method { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
    public Dictionary<string, string> Headers { get; set; }
}