using ChatifyLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Hubs;

public class PrivateChatHub : Hub
{
    public Task SendMessage(PrivateMessageModel message, PrivateConversationModel conversation)
    {
        return Clients.Group(conversation.Id).SendAsync("ReceiveMessage", message);
    }

    public async Task JoinConversation(PrivateConversationModel conversation)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id);
    }

    public async Task LeaveConversation(PrivateConversationModel conversation)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversation.Id);
    }
}
