using ChatifyLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Hubs;

public class TestHub : Hub
{
    public Task SendMessage(MessageModel message)
    {
        return Clients.All.SendAsync("ReceiveMessage", message);
    }
}
