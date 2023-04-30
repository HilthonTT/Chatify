using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatify.Pages.Conversations;

public partial class Conversation
{
    [Parameter]
    public string Id { get; set; }

    [CascadingParameter]
    public EventCallback<MessageModel> RequestChanged { get; set; }

    private CreateMessageModel model = new();
    private HubConnection? hubConnection;
    private string searchText = "";
    private ConversationModel conversation;
    private List<MessageModel> messages;
    private List<MessageModel> nonReadMessages;
    private List<ServerModel> servers;
    private List<BasicUserModel> users;
    private ServerModel selectedServer;
    private UserModel loggedInUser;
    private BasicUserModel selectedUser;
    private BanModel ban;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string errorMessage = "";
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        conversation = await conversationData.GetConversationAsync(Id);
        if (conversation is not null)
        {
            messages = await messageData.GetConversationMessagesAsync(conversation);
        }

        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
            servers = await serverData.GetUserServersAsync(loggedInUser.Id);
            users = await userData.GetUserFriendsAsync(loggedInUser);
        }

        if (loggedInUser is not null && messages is not null)
        {
            nonReadMessages = messages.Where(m => (!m.ReadBy.Any(u => u.Id == loggedInUser.Id)) && m.Sender.Id != loggedInUser?.Id).ToList();
        }

        hubConnection = new HubConnectionBuilder().WithUrl(navManager.ToAbsoluteUri("/conversationhub")).WithAutomaticReconnect().Build();
        hubConnection.On<MessageModel>("ReceiveMessage", OnReceiveMessage);
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinConversation", conversation);
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            if (loggedInUser is not null)
            {
                await FilterUsers();
                await FilterServers();
            }

            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterServers()
    {
        var output = await serverData.GetUserServersAsync(loggedInUser.Id);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(s => s.ServerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || s.ServerDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        servers = output;
        await SaveFilterState();
    }

    private async Task FilterUsers()
    {
        var output = await userData.GetUserFriendsAsync(loggedInUser);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(u => u.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        users = output;
        await SaveFilterState();
    }

    private async Task SendMessage()
    {
        string relativePath = await CaptureFile();
        string objectId = await oidGenerator.GenerateOidAsync();
        MessageModel m = new()
        {
            ObjectIdentifier = objectId,
            Sender = new BasicUserModel(loggedInUser),
            Text = model.Text,
            FileName = relativePath,
            OriginalFileName = fileName,
            FileExtension = fileExtension,
            Conversation = conversation,
            Archived = false,
        };
        m.ReadBy.Add(new BasicUserModel(loggedInUser));
        await messageData.CreateMessage(m);
        if (hubConnection is not null)
        {
            await hubConnection.InvokeAsync("SendMessage", m, conversation);
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
            Conversation = conversation,
            Archived = false,
        };
        m.ReadBy.Add(new BasicUserModel(loggedInUser));
        await messageData.CreateMessage(m);
        if (hubConnection is not null)
        {
            await hubConnection.InvokeAsync("SendMessage", m, conversation);
        }
    }

    private async Task AddParticipant()
    {
        errorMessage = "";
        if (selectedUser is null)
        {
            errorMessage = "You must select a user";
            return;
        }

        conversation.Participants.Add(selectedUser);
        await conversationData.UpdateConversation(conversation);
        users.Remove(users.FirstOrDefault(u => u.Id == selectedUser.Id));
        selectedUser = null;
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private string GetServerClass(ServerModel server)
    {
        if (selectedServer?.Id == server.Id)
        {
            return "in-allowed-roles";
        }

        return "in-disallowed-roles";
    }

    private string GetUserClass(BasicUserModel user)
    {
        if (selectedUser?.Id == user.Id)
        {
            return "in-allowed-roles";
        }

        return "in-disallowed-roles";
    }

    private async Task RemoveParticipant()
    {
        conversation.Participants.Remove(selectedUser);
        await conversationData.UpdateConversation(conversation);
        selectedUser = null;
    }

    private async Task OnServerSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterServers();
    }

    private async Task OnUserSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterUsers();
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void UserDetailsPage(BasicUserModel user)
    {
        navManager.NavigateTo($"/userDetails/{user.Id}");
    }

    private void SettingsPage(ConversationModel conversation)
    {
        navManager.NavigateTo($"/UpdateConversation/{conversation.Id}");
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

    private async Task<string> CaptureFile()
    {
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
        catch
        {
            throw;
        }
    }

    public bool IsConnected => hubConnection?.State == HubConnectionState.Connected; // ? says if not null
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
        }
    }
}