using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class ServerMembers
{
    [Parameter]
    public string Id { get; set; }

    private EditingUserRoleModel editingUserRole = new();
    private ServerModel server;
    private UserModel loggedInUser;
    private UserModel selectedUser;
    private UserModel selectedUserToKick;
    private UserModel selectedUserToBan;
    private UserModel selectedUserToUnban;
    private RoleModel role;
    private BanModel ban;
    private List<UserModel> users;
    private List<RoleModel> roles;
    private string searchText = "";
    private bool priorityBannedUsers = false;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (server is not null)
        {
            users = await userData.GetAllUsersServerAsync(server);
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
                await FilterUsers();
            }

            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(priorityBannedUsers));
        priorityBannedUsers = boolResults.Success ? boolResults.Value : true;
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(priorityBannedUsers), priorityBannedUsers);
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterUsers()
    {
        var output = await userData.GetAllUsersServerAsync(server);
        var bannedUsersId = server.BannedUsers.Select(b => b.Id).ToList();
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(u => u.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (priorityBannedUsers)
        {
            output = output.OrderByDescending(u => IsUserBanned(u)).ToList();
        }
        else
        {
            output = output.OrderByDescending(u => !IsUserBanned(u)).ThenByDescending(u => u.DateCreated).ToList();
        }

        users = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterUsers();
    }

    private async Task OnPriorityClick(bool isPriority)
    {
        priorityBannedUsers = isPriority;
        await FilterUsers();
    }

    private void ClearSelectedUsers()
    {
        selectedUser = null;
        selectedUserToKick = null;
        selectedUserToBan = null;
        selectedUserToUnban = null;
    }

    private async Task KickMember(UserModel user)
    {
        var userToRemove = server.Members.Where(u => u.Id == user.Id).FirstOrDefault();
        server.Members.Remove(userToRemove);
        await serverData.UpdateServer(server);
        ClearSelectedUsers();
    }

    private async Task BanMember(UserModel user)
    {
        var userToBan = server.Members.Where(u => u.Id == user.Id).FirstOrDefault();
        server.BannedUsers.Add(userToBan);
        await serverData.UpdateServer(server);
        ClearSelectedUsers();
    }

    private async Task UnbanMember(UserModel user)
    {
        var userToUnban = server.BannedUsers.Where(u => u.Id == user.Id).FirstOrDefault();
        server.BannedUsers.Remove(userToUnban);
        await serverData.UpdateServer(server);
        ClearSelectedUsers();
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "editRoleModal");
    }

    private async Task UpdateUserRole()
    {
        var user = await userData.GetUserAsync(selectedUser.Id);
        var currentUserRole = roles.Where(r => r.Users.Select(u => u.Id == user.Id).FirstOrDefault()).FirstOrDefault();
        if (currentUserRole is not null)
        {
            var userToRemove = currentUserRole.Users.Where(u => u.Id == user.Id).FirstOrDefault();
            currentUserRole.Users.Remove(userToRemove);
            await roleData.UpdateRole(currentUserRole);
        }

        var newUserRole = roles.Where(r => r.Id == editingUserRole.RoleId).FirstOrDefault();
        AuditLogModel a = new()
        {
            Name = "Change Role",
            Description = $"{loggedInUser.DisplayName} has changed {user.DisplayName} - {user.Id}'s role to {newUserRole.RoleName}",
            OldValues = $"Previous role was {currentUserRole.RoleName}",
            NewValues = $"New role is {newUserRole.RoleName}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        newUserRole.Users.Add(new BasicUserModel(user));
        await roleData.UpdateRole(newUserRole);
        await auditLogData.CreateAuditLog(a);
        selectedUser = null;
        editingUserRole = new();
        await CloseModal();
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private void OpenUserDetails(UserModel user)
    {
        if (user.Id == loggedInUser.Id)
        {
            navManager.NavigateTo("/Profile");
            return;
        }

        navManager.NavigateTo($"/userDetails/{user.Id}");
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private string PriorityBannedClass(bool isPriority)
    {
        if (priorityBannedUsers == isPriority)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private string GetUserRoleName(UserModel user)
    {
        var role = roles.Where(r => r.Users.Any(u => u.Id == user.Id)).Select(r => r.RoleName).FirstOrDefault();
        if (user.Id == server.Owner.Id)
        {
            return "Owner";
        }

        return role ?? "";
    }

    private string GetModalTitleString()
    {
        if (selectedUserToBan is not null)
        {
            return $"Ban {selectedUserToBan.DisplayName}";
        }

        if (selectedUserToKick is not null)
        {
            return $"Kick {selectedUserToKick.DisplayName}";
        }

        return $"Unban {selectedUserToUnban?.DisplayName}";
    }

    private bool IsUserBanned(UserModel user)
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

        if (role is not null && role.CanBanMember || IsUserOwner())
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

        if (role is not null && role.CanKickMember)
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

    private bool HasAnyPermissions()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (CanKickMember())
        {
            return true;
        }

        if (CanBanMember())
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