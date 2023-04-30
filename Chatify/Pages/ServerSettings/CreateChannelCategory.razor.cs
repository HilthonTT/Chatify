using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class CreateChannelCategory
{
    [Parameter]
    public string Id { get; set; }

    private CreateChannelCategoryModel creatingCategoryChannel = new();
    private ServerModel server;
    private RoleModel role;
    private UserModel loggedInUser;
    private BanModel ban;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }

        if (loggedInUser is not null && server is not null)
        {
            role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
        }
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private async Task Create()
    {
        string objectId = await oidGenerator.GenerateOidAsync();
        ChannelCategoryModel c = new()
        {
            ObjectIdentifier = objectId,
            CategoryName = creatingCategoryChannel.CategoryName,
            CategoryDescription = creatingCategoryChannel.CategoryDescription,
            Server = new BasicServerModel(server),
        };
        await channelCategoryData.CreateCategory(c);
        var newCategory = await channelCategoryData.GetCategoryObjectIdAsync(objectId);
        await serverData.UpdateServer(server);
        creatingCategoryChannel = new();
        AuditLogModel a = new()
        {
            Name = "Channel Category Creation",
            Description = $"{loggedInUser.DisplayName} has created channel of Id {newCategory.Id} ",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await auditLogData.CreateAuditLog(a);
        ClosePage();
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