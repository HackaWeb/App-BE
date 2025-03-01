using App.Domain.Models;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace App.Api.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _chatHistory = new();

    public async Task SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Confusion: a note can't be empty.");
            return;
        }

        try
        {
            // TODO: implement openAI integration

            string testResponse = "Hello from backend!";
            await Task.Delay(5000);

            await Clients.Caller.SendAsync("ReceiveResponse", testResponse);
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

    public Task<List<ChatMessage>> LoadChatHistory(string questId)
    {
        return Task.FromResult(_chatHistory.ContainsKey(questId) ? _chatHistory[questId] : new List<ChatMessage>());
    }

    public override Task OnDisconnectedAsync(System.Exception exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    private string GetPrivateChatKey(string user1, string user2)
    {
        return string.Compare(user1, user2, StringComparison.Ordinal) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
    }
}


public class ChatMessage
{
    public int Id { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string QuestId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
