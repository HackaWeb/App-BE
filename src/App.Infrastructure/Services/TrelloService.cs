using App.Application.Services;
using App.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace App.Infrastructure.Services
{
    public class TrelloService(HttpClient httpClient, ILogger<TrelloService> logger) : ITrelloService
    {
        public async Task<OperationResult> CreateProjectAsync(string projectName)
        {
            try
            {
                var requestUri = $"https://api.trello.com/1/boards/?name={Uri.EscapeDataString(projectName)}&key=_apiKey&token=_token";
                var response = await httpClient.PostAsync(requestUri, null);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(jsonResponse);
                    string boardId = json.id;
                    return new OperationResult { Success = true, ProjectId = boardId };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    logger.LogError("Error creating a project in Trello: {Error}", errorContent);
                    return new OperationResult { Success = false, ErrorMessage = errorContent };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a project.");
                return new OperationResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public Task<OperationResult> UpdateProjectNameAsync(string projectId, string newProjectName)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> AddUserToProjectAsync(string projectId, string userName)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> DeleteProjectAsync(string projectId)
        {
            throw new NotImplementedException();
        }
    }
}
