using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.ServerSettings;

public partial class ServerAuditLogs
{
    [Parameter]
    public string Id { get; set; }

    private ServerModel server;
    private UserModel loggedInUser;
    private BanModel ban;
    private RoleModel role;
    private List<AuditLogModel> auditLogs;
    private string searchText = "";
    private bool isSortedByNew = true;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (server is not null)
        {
            auditLogs = await auditLogData.GetAllServerAuditLogsAsync(server);
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
                await FilterAuditLogs();
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

    private async Task FilterAuditLogs()
    {
        var output = await auditLogData.GetAllServerAuditLogsAsync(server);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(a => a.Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || a.Description.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(r => r.DateModified).ToList();
        }
        else
        {
            output = output.OrderBy(r => r.DateModified).ToList();
        }

        auditLogs = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterAuditLogs();
    }

    private async Task SortedByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterAuditLogs();
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isSortedByNew == isNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private bool IsUserRoleNull()
    {
        if (role is null)
        {
            return true;
        }

        return false;
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

    private bool CanBanMember()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanBanMember || IsUserOwner())
        {
            return true;
        }

        return false;
    }

    private bool CanKickMember()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanKickMember)
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

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanCreateChannel)
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

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanCreateRole)
        {
            return true;
        }

        return false;
    }

    private bool CanGiveRole()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanGiveRole)
        {
            return true;
        }

        return false;
    }

    private bool CanViewAuditLog()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanViewAuditLog)
        {
            return true;
        }

        return false;
    }

    private bool CanEditServer()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        if (role.CanEditServer)
        {
            return true;
        }

        return false;
    }

    private bool HasAnyPermission()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (CanBanMember())
        {
            return true;
        }

        if (CanKickMember())
        {
            return true;
        }

        if (CanCreateChannel())
        {
            return true;
        }

        if (CanCreateRole())
        {
            return true;
        }

        if (CanGiveRole())
        {
            return true;
        }

        if (CanViewAuditLog())
        {
            return true;
        }

        if (CanEditServer())
        {
            return true;
        }

        if (IsUserRoleNull())
        {
            return false;
        }

        return false;
    }
}