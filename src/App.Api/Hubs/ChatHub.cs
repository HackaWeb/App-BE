using App.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace App.Api.Hubs;

public class ChatHub(UserManager<User> userManager) : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connectedUsers = new();
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _chatHistory = new();
    private static readonly ConcurrentDictionary<string, List<ChatMessage>> _privateChatHistory = new();

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        
        var userId = httpContext?.Request.Query["userId"];
        var questId = httpContext?.Request.Query["questId"];

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(questId))
        {
            _connectedUsers[userId] = Context.ConnectionId;

            await Groups.AddToGroupAsync(Context.ConnectionId, questId);
            await Clients.Group(questId).SendAsync("ReceiveSystemMessage", $"{userId} joined to {questId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task SendMessageToRoom(string questId, string userId, string message)
    {
        if (!_connectedUsers.TryGetValue(userId, out string? connectionId))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Error: You are not connected to the chat room.");
            return;
        }

        if (connectionId != Context.ConnectionId)
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Error: Invalid attempt to send a message.");
            return;
        }

        var httpContext = Context.GetHttpContext();
        var userQuestId = httpContext?.Request.Query["questId"];

        if (userQuestId.ToString() != questId)
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Error: You are not a member of this chat room.");
            return;
        }

        var chatMessage = new ChatMessage
        {
            SenderUserId = userId,
            QuestId = questId,
            Message = message,
            SentAt = DateTime.UtcNow,
        };

        if (!_chatHistory.ContainsKey(questId))
        {
            _chatHistory[questId] = new List<ChatMessage>();
        }

        if (_chatHistory[questId].Count > 100)
        {
            _chatHistory[questId].RemoveAt(0);
        }

        await Clients.Group(questId).SendAsync("ReceiveMessage", userId, message);
    }

    public async Task SendPrivateMessage(string targetUserId, string message, string questId)
    {
        var senderUserId = _connectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (string.IsNullOrEmpty(senderUserId))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Error: You are not connected.");
            return;
        }

        if (!_connectedUsers.TryGetValue(targetUserId, out string? targetConnectionId))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", $"Error: User {targetUserId} is not online.");
            return;
        }

        var chatMessage = new ChatMessage()
        {
            Message = message, SenderUserId = senderUserId, SentAt = DateTime.UtcNow, QuestId = questId
        };

        string chatKey = GetPrivateChatKey(senderUserId, targetUserId);

        if (!_privateChatHistory.ContainsKey(chatKey))
        {
            _privateChatHistory[chatKey] = new List<ChatMessage>();
        }

        _privateChatHistory[chatKey].Add(chatMessage);

        if (_privateChatHistory[chatKey].Count > 50)
        {
            _privateChatHistory[chatKey].RemoveAt(0);
        }

        await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", senderUserId, message);
        await Clients.Caller.SendAsync("ReceivePrivateMessage", $"To {targetUserId}", message);
    }

    public Task<List<ChatMessage>> LoadChatHistory(string questId)
    {
        return Task.FromResult(_chatHistory.ContainsKey(questId) ? _chatHistory[questId] : new List<ChatMessage>());
    }

    public override Task OnDisconnectedAsync(System.Exception exception)
    {
        var userId = _connectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (!string.IsNullOrEmpty(userId))
        {
            _connectedUsers.TryRemove(userId, out _);
        }

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
