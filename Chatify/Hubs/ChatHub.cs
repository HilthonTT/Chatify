using ChatifyLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Hubs;

public class ChatHub : Hub
{
    public Task SendMessage(MessageModel message, ConversationModel conversation)
    {
        return Clients.Group(conversation.Id).SendAsync("ReceiveMessage", message);

        //return Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task JoinConversation(ConversationModel conversation)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id);
    }

    public async Task LeaveConversation(ConversationModel conversation)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversation.Id);
    }
}
