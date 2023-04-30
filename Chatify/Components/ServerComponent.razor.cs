using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;

namespace Chatify.Components;

public partial class ServerComponent
{
    [Parameter]
    [EditorRequired]
    public ServerModel Server { get; set; }

    [Parameter]
    [EditorRequired]
    public UserModel LoggedInUser { get; set; }

    private ServerModel selectedServer;
    private List<MessageModel> nonReadMessages;
    protected override async Task OnInitializedAsync()
    {
        nonReadMessages = await messageData.GetServerUnreadMessagesAsync(Server, LoggedInUser);
    }

    private async Task CloseModal(ServerModel server)
    {
        await JSRuntime.InvokeVoidAsync("closeModal", $"modal-{server.Id}");
    }

    private async Task JoinServer(ServerModel server)
    {
        var user = new BasicUserModel(LoggedInUser);
        var role = await roleData.GetServerMemberRoleAsync(server);
        role.Users.Add(user);
        var serverRole = await roleData.GetServerMemberRoleAsync(server);
        serverRole.Users.Add(user);
        await roleData.UpdateRole(role);
        await roleData.UpdateRole(serverRole);
        server.Members.Add(user);
        await serverData.UpdateServer(server);
        OpenDetails(server);
    }

    private async Task LeaveServer(ServerModel server)
    {
        var userToRemove = server.Members.FirstOrDefault(m => m.Id == LoggedInUser?.Id);
        var userServerRole = await roleData.GetUserServerRoleAsync(LoggedInUser, Server);
        if (userServerRole is not null)
        {
            var userToRoleRemove = userServerRole.Users.Where(u => u.Id == LoggedInUser.Id).FirstOrDefault();
            userServerRole.Users.Remove(userToRoleRemove);
            await roleData.UpdateRole(userServerRole);
        }

        if (userToRemove is not null)
        {
            server.Members.Remove(userToRemove);
            await serverData.UpdateServer(server);
        }

        await CloseModal(server);
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private void OpenDetails(ServerModel server)
    {
        navManager.NavigateTo($"/Server/{server.Id}");
    }

    private bool IsUserBanned(ServerModel server)
    {
        bool IsUserInBannedList = server.BannedUsers.Any(b => b.Id == LoggedInUser?.Id);
        if (IsUserInBannedList)
        {
            return true;
        }

        return false;
    }

    private bool IsUserInServer(ServerModel server)
    {
        if (server.Owner.Id == LoggedInUser?.Id || server.Members.Any(m => m.Id == LoggedInUser?.Id))
        {
            return true;
        }

        return false;
    }

    private string GetNonReadMessagesCount()
    {
        if (nonReadMessages?.Count == 0)
        {
            return "";
        }

        if (nonReadMessages?.Count == 1)
        {
            return "1 unread message";
        }

        if (nonReadMessages?.Count > 99)
        {
            return "99+ unread messages";
        }

        if (nonReadMessages?.Count < 99)
        {
            return $"{nonReadMessages.Count} unread messages";
        }

        return "";
    }
}