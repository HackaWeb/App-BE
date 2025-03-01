namespace App.Application.Services
{
    public interface IOpenAIService
    {
        Task<string> GetChatCompletionAsync(string prompt);
    }
}
