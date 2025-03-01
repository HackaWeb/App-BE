using App.Application.Services;
using App.Domain;
using App.Domain.Exceptions;
using App.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace App.Infrastructure.Services
{
    public class OpenAIService(
        HttpClient httpClient, 
        IOptions<OpenAISettings> settings) : IOpenAIService
    {
        public async Task<string> GetChatCompletionAsync(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                throw new DomainException("Prompt cannot be empty.", (int)HttpStatusCode.BadRequest);
            }

            var requestData = new
            {
                model = settings.Value.Model,
                messages = new[] { new { role = "user", content = prompt }, },
                temperature = 0.7,
            };

            var jsonContent = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var apiKey = Environment.GetEnvironmentVariable(AppConstants.OPEN_AI_API_KEY)
                         ?? settings.Value.ApiKey;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                return result.choices[0].message.content.ToString();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new DomainException($"OpenAI API error: {errorContent}", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
