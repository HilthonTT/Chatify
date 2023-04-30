using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class ServerChannels
{
    [Parameter]
    public string Id { get; set; }

    private CreateChannelModel editingChannel = new();
    private ServerModel server;
    private UserModel loggedInUser;
    private BanModel ban;
    private ChannelModel selectedChannel;
    private ChannelModel archivingChannel;
    private RoleModel role;
    private List<ChannelModel> channels;
    private string searchText = "";
    private bool isSortedByNew = true;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (server is not null)
        {
            channels = await channelData.GetAllChannelsServerAsync(server);
        }

        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }

        if (loggedInUser is not null && server is not null)
        {
            role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            if (server is not null)
            {
                await FilterChannels();
            }

            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterChannels()
    {
        var output = await channelData.GetAllChannelsServerAsync(server);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(c => c.ChannelName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || c.ChannelDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(c => c.DateCreated).ToList();
        }
        else
        {
            output = output.OrderBy(c => c.DateCreated).ToList();
        }

        channels = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterChannels();
    }

    private async Task SortedByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterChannels();
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "channelModal");
    }

    private async Task UpdateChannel()
    {
        string oldChannelName = selectedChannel.ChannelName;
        string oldCHannelDescription = selectedChannel.ChannelDescription;
        selectedChannel.ChannelName = editingChannel.ChannelName;
        selectedChannel.ChannelDescription = editingChannel.ChannelDescription;
        var serverChannel = await channelData.GetChannelAsync(selectedChannel.Id);
        serverChannel.ChannelName = selectedChannel.ChannelName;
        serverChannel.ChannelDescription = selectedChannel.ChannelDescription;
        await serverData.UpdateServer(server);
        AuditLogModel a = new()
        {
            Name = "Update Channel",
            Description = $"{loggedInUser.DisplayName} has updated channel {oldChannelName} to {selectedChannel.ChannelName}",
            OldValues = $"Channel Name: {oldChannelName}, Channel Description: {oldChannelName}",
            NewValues = $"Channel Name: {selectedChannel.ChannelName}, Channel Description: {selectedChannel.ChannelDescription}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await channelData.UpdateChannel(selectedChannel);
        await auditLogData.CreateAuditLog(a);
        selectedChannel = null;
        editingChannel = new();
        await CloseModal();
    }

    private async Task ArchiveChannel()
    {
        archivingChannel.Archived = true;
        await channelData.UpdateChannel(archivingChannel);
        channels.Remove(channels.FirstOrDefault(c => c.Id == archivingChannel.Id));
        await CloseModal();
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private void LoadSelectedChannel(ChannelModel model)
    {
        selectedChannel = model;
        editingChannel.ChannelName = model.ChannelName;
        editingChannel.ChannelDescription = model.ChannelDescription;
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isSortedByNew == isNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private bool IsUserOwner()
    {
        if (loggedInUser is null || server is null)
        {
            return false;
        }

        if (loggedInUser.Id == server.Owner.Id)
        {
            return true;
        }

        return false;
    }

    private bool CanCreateChannel()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (role is not null && role.CanCreateChannel)
        {
            return true;
        }

        return false;
    }

    private bool HasAnyPermissions()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (CanCreateChannel())
        {
            return true;
        }

        return false;
    }
}