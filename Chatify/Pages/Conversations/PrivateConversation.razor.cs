using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatify.Pages.Conversations;

public partial class PrivateConversation
{
    [Parameter]
    public string Id { get; set; }

    [CascadingParameter]
    public EventCallback<MessageModel> RequestChanged { get; set; }

    private CreateMessageModel model = new();
    private HubConnection? hubConnection;
    private string searchText = "";
    private bool isSortedByNew = true;
    private PrivateConversationModel conversation;
    private List<MessageModel> messages;
    private List<MessageModel> nonReadMessages;
    private List<ServerModel> servers;
    private ServerModel selectedServer;
    private UserModel loggedInUser;
    private BanModel ban;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string errorMessage = "";
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        conversation = await privateConversationData.GetConversationAsync(Id);
        if (conversation is not null)
        {
            messages = await messageData.GetPrivateConversationMessagesAsync(conversation);
        }

        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
            servers = await serverData.GetUserServersAsync(loggedInUser.Id);
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
            if (conversation is not null)
            {
                await FilterMessages();
            }

            StateHasChanged();
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
        await sessionStorage.SetAsync(nameof(searchText), searchText);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
    }

    private async Task FilterMessages()
    {
        var output = await messageData.GetPrivateConversationMessagesAsync(conversation);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(m => m.Text.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(c => c.Timestamp).ToList();
        }
        else
        {
            output = output.OrderBy(c => c.Timestamp).ToList();
        }

        messages = output;
        await SaveFilterState();
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

    private async Task SendMessage()
    {
        try
        {
            errorMessage = "";
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
                PrivateConversation = conversation,
            };
            m.ReadBy.Add(new BasicUserModel(loggedInUser));
            await messageData.CreateMessage(m);
            if (hubConnection is not null)
            {
                await hubConnection.InvokeAsync("SendMessage", m, conversation);
            }
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
            PrivateConversation = conversation,
            Archived = false,
        };
        await messageData.CreateMessage(m);
        if (hubConnection is not null)
        {
            await hubConnection.InvokeAsync("SendMessage", m, conversation);
        }
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "messageModal");
    }

    private async Task UpdateMessage(MessageModel message)
    {
        string relativePath = await CaptureFile();
        string oldFileName = message.FileName;
        string oldOriginalFile = message.OriginalFileName;
        string oldExtension = message.FileExtension;
        message.Text = model.Text;
        message.LastEdited = DateTime.UtcNow;
        if (file is not null)
        {
            message.FileName = relativePath;
            message.OriginalFileName = fileName;
            message.FileExtension = fileExtension;
        }
        else
        {
            message.FileName = oldFileName;
            message.OriginalFileName = oldOriginalFile;
            message.FileExtension = oldExtension;
        }

        await messageData.UpdateMessageAsync(message);
        model = new();
        file = null;
        await CloseModal();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterMessages();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterMessages();
        await FilterServers();
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void UserDetailsPage(BasicUserModel user)
    {
        navManager.NavigateTo($"/userDetails/{user.Id}");
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

    private string GetServerClass(ServerModel server)
    {
        if (selectedServer?.Id == server.Id)
        {
            return "in-allowed-roles";
        }

        return "in-disallowed-roles";
    }

    private bool IsInConversation()
    {
        if (conversation.FirstParticipant.Id != loggedInUser.Id && conversation.LastParticipant.Id != loggedInUser.Id)
        {
            return false;
        }

        return true;
    }

    private bool IsBlocked()
    {
        var blockedUser = loggedInUser?.BlockedUsers.FirstOrDefault(u => ((u.Id == conversation.FirstParticipant.Id && u.Id != conversation.LastParticipant.Id) || (u.Id == conversation.LastParticipant.Id && u.Id != conversation.FirstParticipant.Id)) && u.Id != loggedInUser?.Id);
        if (blockedUser is not null)
        {
            return true;
        }

        return false;
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