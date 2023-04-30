using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatify.Pages.Servers;

public partial class Server
{
    [Parameter]
    public string Id { get; set; }

    [CascadingParameter]
    public EventCallback<MessageModel> RequestChanged { get; set; }

    private CreateMessageModel model = new();
    private HubConnection? hubConnection;
    private string searchText = "";
    private bool isSortedByNew = true;
    private ServerModel server;
    private ServerModel selectedServer;
    private ChannelModel selectedChannel;
    private List<MessageModel> messages;
    private List<MessageModel> nonReadMessages;
    private List<ChannelCategoryModel> channelCategories;
    private List<ChannelModel> channels;
    private List<RoleModel> roles;
    private List<ServerModel> servers;
    private UserModel loggedInUser;
    private RoleModel role;
    private BanModel ban;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile file;
    private string errorMessage = "";
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        if (server is not null)
        {
            channels = await channelData.GetAllChannelsServerAsync(server);
            selectedChannel = channels.FirstOrDefault();
            messages = await messageData.GetChannelMessagesAsync(selectedChannel);
            roles = await roleData.GetAllRolesServerAsync(server);
            channelCategories = await channelCategoryData.GetServerCategoriesAsync(server);
        }

        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
            servers = await serverData.GetUserServersAsync(loggedInUser.Id);
            if (server is not null)
                role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
        }

        LoadNonReadMessages();
        if (selectedChannel is not null)
        {
            hubConnection = new HubConnectionBuilder().WithUrl(navManager.ToAbsoluteUri("/channelhub")).WithAutomaticReconnect().Build();
            hubConnection.On<MessageModel>("ReceiveMessage", OnReceiveMessage);
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("JoinConversation", selectedChannel);
        }
    }

    private void OnReceiveMessage(MessageModel message)
    {
        file = null;
        fileName = "";
        fileExtension = "";
        model = new();
        messages.Add(message);
        InvokeAsync(StateHasChanged);
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "messageModal");
    }

    private async Task LoadSelectedChannel(ChannelModel channel)
    {
        if (hubConnection is not null)
        {
            await DisposeHubConnection();
        }

        hubConnection = new HubConnectionBuilder().WithUrl(navManager.ToAbsoluteUri("/channelhub")).WithAutomaticReconnect().Build();
        selectedChannel = channel;
        messages = await messageData.GetChannelMessagesAsync(selectedChannel);
        LoadNonReadMessages();
        hubConnection.On<MessageModel>("ReceiveMessage", OnReceiveMessage);
        if (hubConnection.State == HubConnectionState.Disconnected)
        {
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("JoinConversation", selectedChannel);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            if (loggedInUser is not null)
            {
                await FilterServers();
            }

            StateHasChanged();
        }
    }

    private void LoadNonReadMessages()
    {
        if (loggedInUser is not null && messages is not null)
        {
            nonReadMessages = messages.Where(m => (!m.ReadBy.Any(u => u.Id == loggedInUser.Id)) && m.Sender.Id != loggedInUser?.Id).ToList();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterServers()
    {
        var output = await serverData.GetUserServersAsync(loggedInUser.Id);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(s => s.ServerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || s.ServerDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(s => s.DateCreated).ToList();
        }
        else
        {
            output = output.OrderBy(s => s.DateCreated).ToList();
        }

        servers = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterServers();
    }

    private async Task SendMessage()
    {
        try
        {
            errorMessage = "";
            string relativePath = await CaptureFile();
            string objectIdentifier = await oidGenerator.GenerateOidAsync();
            MessageModel m = new()
            {
                ObjectIdentifier = objectIdentifier,
                Sender = new BasicUserModel(loggedInUser),
                Text = model.Text,
                FileName = relativePath,
                OriginalFileName = fileName,
                FileExtension = fileExtension,
                Channel = selectedChannel,
                Server = server,
                Archived = false,
            };
            m.ReadBy.Add(new BasicUserModel(loggedInUser));
            if (hubConnection is not null)
            {
                await hubConnection.InvokeAsync("SendMessage", m, selectedChannel);
            }

            await messageData.CreateMessage(m);
            var newMessage = await messageData.GetMessageObjectIdentifierAsync(m);
            selectedChannel.Messages.Add(newMessage);
            await channelData.UpdateChannel(selectedChannel);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task SendInvite()
    {
        errorMessage = "";
        if (selectedServer is null)
        {
            errorMessage = "You must select a server.";
            return;
        }

        string objectId = await oidGenerator.GenerateOidAsync();
        MessageModel m = new()
        {
            ObjectIdentifier = objectId,
            Sender = new BasicUserModel(loggedInUser),
            Text = $"I've just invited you in {selectedServer.ServerName}",
            ServerInvitation = selectedServer,
            Channel = selectedChannel,
            Server = server,
            Archived = false,
        };
        if (hubConnection is not null)
        {
            await hubConnection.InvokeAsync("SendMessage", m, selectedChannel);
        }

        await messageData.CreateMessage(m);
        var newMessage = await messageData.GetMessageObjectIdentifierAsync(m);
        selectedChannel.Messages.Add(newMessage);
        await channelData.UpdateChannel(selectedChannel);
    }

    private string GetUserRole(BasicUserModel user)
    {
        if (user.Id == server?.Owner.Id)
        {
            return "Owner";
        }

        var role = roles?.FirstOrDefault(r => r.Users.Any(u => u.Id == user.Id));
        return role?.RoleName;
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void SettingsPage(ServerModel conversation)
    {
        navManager.NavigateTo($"/ServerSettings/{conversation.Id}");
    }

    private void UserDetailsPage(BasicUserModel user)
    {
        if (user.Id == loggedInUser.Id)
        {
            navManager.NavigateTo($"/profile");
            return;
        }

        navManager.NavigateTo($"/userDetails/{user.Id}");
    }

    private void InviteFriendPage(ServerModel server)
    {
        navManager.NavigateTo($"/InviteFriend/Server/{server.Id}");
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private void GetFileExtensionAndFileName()
    {
        fileName = file.Name;
        fileExtension = Path.GetExtension(file.Name);
    }

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        file = e.File;
        GetFileExtensionAndFileName();
    }

    private async Task<string> CaptureFile()
    {
        errorMessage = "";
        if (file is null || loggedInUser is null)
            return "";
        try
        {
            string newFileName = Path.ChangeExtension(Path.GetRandomFileName(), Path.GetExtension(file.Name));
            string path = Path.Combine(config.GetValue<string>("FileStorage"), loggedInUser.Email, newFileName);
            string relativePath = Path.Combine(loggedInUser.Email, newFileName);
            Directory.CreateDirectory(Path.Combine(config.GetValue<string>("FileStorage"), loggedInUser.Email));
            await using FileStream fs = new(path, FileMode.Create);
            await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
            return relativePath;
        }
        catch (Exception ex)
        {
            errorMessage = $"File: {file.Name} Error: {ex.Message}";
            throw;
        }
    }

    private string GetChannelClass(ChannelModel channel)
    {
        if (channel.Id == selectedChannel.Id)
        {
            return "bg-secondary";
        }

        return "bg-dark";
    }

    private string GetMemberClass(BasicUserModel user)
    {
        if (user.Id == server?.Owner.Id)
        {
            return "text-warning";
        }

        return "";
    }

    private string GetServerClass(ServerModel server)
    {
        if (selectedServer?.Id == server.Id)
        {
            return "in-allowed-roles";
        }

        return "in-disallowed-roles";
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

    private bool IsUserAuthorized()
    {
        if (server is not null && loggedInUser is not null)
        {
            bool IsUserInServer = server.Members.Any(m => m.Id == loggedInUser.Id);
            bool IsUserInBannedList = server.BannedUsers.Any(b => b.Id == loggedInUser.Id);
            if (IsUserInBannedList)
            {
                return false;
            }

            if (IsUserInServer || server.Owner.Id == loggedInUser.Id)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsUserBanned(BasicUserModel user)
    {
        if (server is not null)
        {
            bool IsUserInBannedList = server.BannedUsers.Any(b => b.Id == user.Id);
            if (IsUserInBannedList)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private bool UserHasPermission()
    {
        if (loggedInUser is null)
        {
            // If no user is logged in, they do not have permission to access the server
            return false;
        }

        // Check if the logged-in user is the owner of the server
        if (loggedInUser.Id == server.Owner.Id)
        {
            // If so, the user has permission to access the server
            return true;
        }

        // If the logged-in user is not the owner, check if they have a role with the necessary permissions
        if (role is not null)
        {
            // Check if the user has any of the following role-based permissions: ban member, kick member, create channel, create role, give role, view audit log, or edit server
            if (role.CanBanMember || role.CanKickMember || role.CanCreateChannel || role.CanCreateRole || role.CanGiveRole || role.CanViewAuditLog || role.CanEditServer)
            {
                // If so, the user has permission to access the server
                return true;
            }
        }

        // If the user does not have any of the necessary permissions, they do not have permission to access the server
        return false;
    }

    private bool IsUserAllowedToChat(ChannelModel channel)
    {
        if (loggedInUser is null || server is null || selectedChannel is null)
        {
            return false;
        }

        if (loggedInUser.Id == server.Owner.Id)
        {
            return true;
        }

        // Checks if the user's role is in the allowed roles of the channel
        if (selectedChannel.AllowedRoles.Where(r => r.Id == role.Id).FirstOrDefault()is not null)
        {
            return true;
        }

        return false;
    }

    private async Task DisposeHubConnection()
    {
        if (hubConnection is not null)
        {
            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
        }
    }

    public bool IsConnected => hubConnection?.State == HubConnectionState.Connected; // ? says if not null
    public async ValueTask DisposeAsync()
    {
        await DisposeHubConnection();
    }
}