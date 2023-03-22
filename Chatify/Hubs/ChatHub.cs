using ChatifyLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(ConversationModel conversation,
                                  UserModel user,
                                  MessageModel message)
    {
        await Clients.Group(
            conversation.Id).SendAsync(
            "ReceiveMessage", $"{user.FirstName} {user.LastName}", message.Text);
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
