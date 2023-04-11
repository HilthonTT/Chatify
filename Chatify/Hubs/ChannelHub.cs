using ChatifyLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Hubs;

public class ChannelHub : Hub
{
    public Task SendMessage(MessageModel message, ChannelModel channel)
    {
        return Clients.Group(channel.Id).SendAsync("ReceiveMessage", message);
    }

    public async Task JoinConversation(ChannelModel channel)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channel.Id);
    }

    public async Task LeaveConversation(ChannelModel channel)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel.Id);
    }
}
