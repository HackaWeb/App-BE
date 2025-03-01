using App.Domain.Models;

namespace App.Application.Services
{
    public interface ITrelloService
    {
        Task<OperationResult> CreateProjectAsync(string projectName);

        Task<OperationResult> UpdateProjectNameAsync(string projectId, string newProjectName);

        Task<OperationResult> AddUserToProjectAsync(string projectId, string userName);

        Task<OperationResult> DeleteProjectAsync(string projectId);
    }
}
