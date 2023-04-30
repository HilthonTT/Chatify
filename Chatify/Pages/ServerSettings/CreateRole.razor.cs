using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class CreateRole
{
    [Parameter]
    public string Id { get; set; }

    private CreateRoleModel creatingRole = new();
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

        if (server is not null && loggedInUser is not null)
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
        RoleModel r = new()
        {
            RoleName = creatingRole.RoleName,
            RoleDescription = creatingRole.RoleDescription,
            Server = server,
            CanBanMember = creatingRole.CanBanMember,
            CanKickMember = creatingRole.CanKickMember,
            CanCreateChannel = creatingRole.CanCreateChannel,
            CanGiveRole = creatingRole.CanGiveRole,
            CanViewAuditLog = creatingRole.CanViewAuditLog,
            CanEditServer = creatingRole.CanEditServer,
        };
        await roleData.CreateRole(r);
        creatingRole = new();
        AuditLogModel a = new()
        {
            Name = "Role Creation",
            Description = $"{loggedInUser.DisplayName} has created role {r.RoleName}",
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

    private bool CanCreateRole()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (role is not null && role.CanCreateRole)
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

        if (CanCreateRole())
        {
            return true;
        }

        return false;
    }
}