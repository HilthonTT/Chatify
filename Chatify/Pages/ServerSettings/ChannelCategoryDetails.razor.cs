using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class ChannelCategoryDetails
{
    [Parameter]
    public string Id { get; set; }

    private CreateChannelModel creatingChannel = new();
    private UserModel loggedInUser;
    private ChannelCategoryModel channelCategory;
    private ServerModel server;
    private List<ChannelModel> channels;
    private BanModel ban;
    private RoleModel role;
    private List<RoleModel> roles;
    private List<RoleModel> allowedChannelRoles = new();
    private List<RoleModel> disallowedChannelRoles = new();
    private string searchText = "";
    private bool showCreateChannel = false;
    protected override async Task OnInitializedAsync()
    {
        channelCategory = await channelCategoryData.GetCategoryAsync(Id);
        server = await serverData.GetServerAsync(channelCategory.Server.Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (channelCategory is not null)
        {
            channels = await channelData.GetAllChannelsCategoryAsync(channelCategory);
        }

        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }

        if (server is not null && loggedInUser is not null)
        {
            role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
            roles = await roleData.GetAllRolesServerAsync(server);
            allowedChannelRoles = new List<RoleModel>(roles);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            if (server is not null)
            {
                await FilterRoles();
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

    private async Task FilterRoles()
    {
        var output = await roleData.GetAllRolesServerAsync(server);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(r => r.RoleName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || r.RoleDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        roles = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterRoles();
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private async Task Create()
    {
        string objectId = await oidGenerator.GenerateOidAsync();
        ChannelModel c = new()
        {
            ObjectIdentifier = objectId,
            Category = channelCategory,
            ChannelName = creatingChannel.ChannelName,
            ChannelDescription = creatingChannel.ChannelDescription,
            Server = server,
            AllowedRoles = allowedChannelRoles,
            DisallowedRoles = disallowedChannelRoles,
        };
        await channelData.CreateChannel(c);
        var newChannel = await channelData.GetChannelObjectIdAsync(objectId);
        channelCategory.Channels.Add(newChannel);
        await serverData.UpdateServer(server);
        await channelCategoryData.UpdateCategory(channelCategory);
        creatingChannel = new();
        AuditLogModel a = new()
        {
            Name = "Channel Creation",
            Description = $"{loggedInUser.DisplayName} has created channel of Id {newChannel.Id}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await auditLogData.CreateAuditLog(a);
        allowedChannelRoles.Clear();
        disallowedChannelRoles.Clear();
        ClosePage();
    }

    private void AddOrRemoveSelectedRole(RoleModel role)
    {
        var allowedRole = allowedChannelRoles.Where(r => r.Id == role.Id).FirstOrDefault();
        var disallowedRole = disallowedChannelRoles.Where(r => r.Id == role.Id).FirstOrDefault();
        if (allowedRole is null && disallowedRole is null)
        {
            allowedChannelRoles.Add(role);
        }
        else if (allowedRole is not null)
        {
            allowedChannelRoles.Remove(role);
            disallowedChannelRoles.Add(role);
        }
        else if (disallowedRole is not null)
        {
            disallowedChannelRoles.Remove(role);
            allowedChannelRoles.Add(role);
        }
        else
        {
            throw new Exception("Unexpected condition");
        }
    }

    private string GetRoleClass(RoleModel role)
    {
        if (allowedChannelRoles.Contains(role))
        {
            return "in-allowed-roles";
        }

        return "in-disallowed-roles";
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