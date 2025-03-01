using App.Application.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace App.Api.Hubs;

public class ChatHub(IOpenAIService openAIService) : Hub
{
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _chatHistory = new();

    public async Task SendMessage(string userId, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Confusion: a note can't be empty.");
            return;
        }

        try
        {
            var chatMessage = new ChatMessage() { Sender = "User", SentAt = DateTime.UtcNow, Message = message, };

            _chatHistory.AddOrUpdate(userId, key => new List<ChatMessage> { chatMessage },
                (key, existingList) =>
                {
                    existingList.Add(chatMessage);
                    if (existingList.Count > 50)
                    {
                        int removeCount = existingList.Count - 50;
                        existingList.RemoveRange(0, removeCount);
                    }
                    return existingList;
                });


            var botResponse = await openAIService.GetChatCompletionAsync(message);

            var botMessage = new ChatMessage { Sender = "Bot", SentAt = DateTime.UtcNow, Message = botResponse, };

            _chatHistory.AddOrUpdate(userId,
                key => new List<ChatMessage> { botMessage },
                (key, existingList) =>
                {
                    existingList.Add(botMessage);
                    if (existingList.Count > 50)
                    {
                        int removeCount = existingList.Count - 50;
                        existingList.RemoveRange(0, removeCount);
                    }
                    return existingList;
                });

            await Clients.Caller.SendAsync("ReceiveResponse", botResponse);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", $"Error while processing a request: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveSystemMessage", "The connection is established. You can send requests.");
        await base.OnConnectedAsync();
    }

    public Task<List<ChatMessage>> LoadChatHistory(string userId)
    {
        var history = _chatHistory.ContainsKey(userId) ? _chatHistory[userId] : new List<ChatMessage>();
        var sortedHistory = history.OrderBy(msg => msg.SentAt).ToList();
        return Task.FromResult(sortedHistory);
    }

    public async Task CleanHistory(string userId)
    {
        _chatHistory.TryRemove(userId, out _);
        await Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(System.Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}


public class ChatMessage
{
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
