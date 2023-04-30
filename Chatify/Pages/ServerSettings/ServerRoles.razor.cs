using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class ServerRoles
{
    [Parameter]
    public string Id { get; set; }

    private CreateRoleModel editingRole = new();
    private ServerModel server;
    private UserModel loggedInUser;
    private BanModel ban;
    private RoleModel selectedRole;
    private RoleModel archivingRole;
    private RoleModel role;
    private List<RoleModel> roles;
    private string searchText = "";
    private bool isSortedByNew = true;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (server is not null)
        {
            roles = await roleData.GetAllRolesServerAsync(server);
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
                await FilterRoles();
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

    private async Task FilterRoles()
    {
        var output = await roleData.GetAllRolesServerAsync(server);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(r => r.RoleName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || r.RoleDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(r => r.DateCreated).ToList();
        }
        else
        {
            output = output.OrderBy(r => r.DateCreated).ToList();
        }

        roles = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterRoles();
    }

    private async Task SortedByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterRoles();
    }

    private async Task GiveRole(UserModel user, RoleModel role)
    {
        role.Users.Add(new BasicUserModel(user));
        await roleData.UpdateRole(role);
    }

    private async Task RemoveRole(UserModel user, RoleModel role)
    {
        var userToRemove = role.Users.Where(u => u.Id == user.Id).FirstOrDefault();
        role.Users.Remove(userToRemove);
        await roleData.UpdateRole(role);
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "roleModal");
    }

    private async Task UpdateRole()
    {
        var oldRole = await roleData.GetRoleAsync(selectedRole.Id);
        if (oldRole is null)
            return;
        selectedRole.RoleName = editingRole.RoleName;
        selectedRole.RoleDescription = editingRole.RoleDescription;
        selectedRole.CanBanMember = editingRole.CanBanMember;
        selectedRole.CanKickMember = editingRole.CanKickMember;
        selectedRole.CanCreateRole = editingRole.CanCreateRole;
        selectedRole.CanCreateChannel = editingRole.CanCreateRole;
        selectedRole.CanGiveRole = editingRole.CanGiveRole;
        selectedRole.CanViewAuditLog = editingRole.CanViewAuditLog;
        selectedRole.CanEditServer = editingRole.CanEditServer;
        AuditLogModel a = new()
        {
            Name = "Server Details Updated",
            Description = $"{loggedInUser.DisplayName} has updated {selectedRole.RoleName}'s details",
            OldValues = $@"Role name: {oldRole.RoleName}  
                Role Description: {oldRole.RoleDescription}",
            NewValues = $@"Role name: {selectedRole.RoleName} 
                Role Description: {selectedRole.RoleDescription}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await roleData.UpdateRole(selectedRole);
        await auditLogData.CreateAuditLog(a);
        await CloseModal();
        role = new();
        selectedRole = null;
    }

    private async Task ArchiveRole()
    {
        archivingRole.Archived = true;
        await roleData.UpdateRole(archivingRole);
        roles.Remove(roles.FirstOrDefault(r => r.Id == archivingRole.Id));
        archivingRole = null;
        await CloseModal();
    }

    private void ReturnSettings()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private void LoadSelectedRole(RoleModel model)
    {
        selectedRole = model;
        editingRole.RoleName = model.RoleName;
        editingRole.RoleDescription = model.RoleDescription;
        editingRole.CanBanMember = model.CanBanMember;
        editingRole.CanKickMember = model.CanKickMember;
        editingRole.CanCreateChannel = model.CanCreateChannel;
        editingRole.CanCreateRole = model.CanCreateRole;
        editingRole.CanGiveRole = model.CanGiveRole;
        editingRole.CanViewAuditLog = model.CanViewAuditLog;
        editingRole.CanEditServer = model.CanEditServer;
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isSortedByNew == isNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private bool HasUserGotRole(UserModel user, RoleModel role)
    {
        if (role.Users.Where(u => u.Id == user.Id).FirstOrDefault()is not null)
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

    private bool CanGiveRole()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (role is not null && role.CanGiveRole)
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

        if (CanGiveRole())
        {
            return true;
        }

        return false;
    }
}