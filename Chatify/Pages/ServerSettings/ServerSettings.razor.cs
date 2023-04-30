using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Chatify;
using Chatify.Shared;
using ChatifyLibrary.DataAccess;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;
namespace Chatify.Pages.ServerSettings;

public partial class ServerSettings
{
    [Parameter]
    public string Id { get; set; }

    private CreateServerModel editingServer = new();
    private ServerModel server;
    private ServerModel archivingServer;
    private UserModel loggedInUser;
    private BanModel ban;
    private RoleModel role;
    private List<CategoryModel> categories;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        categories = await categoryData.GetAllCategoriesAsync();
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        server = await serverData.GetServerAsync(Id);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
            role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
        }

        editingServer.ServerName = server.ServerName;
        editingServer.ServerDescription = server.ServerDescription;
        editingServer.PictureName = server.PictureName;
        if (server is not null)
        {
            editingServer.CategoryId = categories.Where(c => c.Id == server.Category.Id).FirstOrDefault().Id;
        }
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "serverModal");
    }

    private async Task UpdateServer()
    {
        string relativePath = await CaptureFile();
        string serverName = server.ServerName;
        string serverDescription = server.ServerDescription;
        string pictureName = server.PictureName;
        string originalPictureName = server.OriginalPictureName;
        string pictureExtension = server.PictureExtension;
        server.ServerName = editingServer.ServerName;
        server.ServerDescription = editingServer.ServerDescription;
        server.Category = categories.Where(c => c.Id == editingServer.CategoryId).FirstOrDefault();
        if (file is not null)
        {
            server.PictureName = relativePath;
            server.OriginalPictureName = fileName;
            server.PictureExtension = fileExtension;
        }
        else
        {
            server.PictureName = pictureName;
            server.OriginalPictureName = originalPictureName;
            server.PictureExtension = pictureExtension;
        }

        AuditLogModel a = new()
        {
            Name = "Server Details Updated",
            Description = $"{loggedInUser.DisplayName} has updated server of ID {server.Id}'s details",
            OldValues = $@"Server name: {serverName}  
                Server Description: {serverDescription} 
                Server Picture: {pictureName}
                Original picture name: {originalPictureName}",
            NewValues = $@"Server name: {server.ServerName} 
                Server Description: {server.ServerDescription} 
                Server Picture: {server.PictureName} 
                Original picture name: {server.OriginalPictureName}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await serverData.UpdateServer(server);
        await auditLogData.CreateAuditLog(a);
        await CloseModal();
        file = null;
    }

    private async Task ArchiveServer()
    {
        server.Archived = true;
        await serverData.UpdateServer(server);
        await CloseModal();
        ClosePage();
    }

    private void ClearPicture()
    {
        server.PictureName = "";
        server.OriginalPictureName = "";
        server.PictureExtension = "";
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/Servers");
    }

    private void ReturnSettings()
    {
        navManager.NavigateTo($"/Server/{server.Id}");
    }

    private void OpenMembersPage()
    {
        navManager.NavigateTo($"/ServerSettings/Members/{server.Id}");
    }

    private void OpenChannelsPage()
    {
        navManager.NavigateTo($"/ServerSettings/Channels/{server.Id}");
    }

    private void OpenAuditLogsPage()
    {
        navManager.NavigateTo($"/ServerSettings/AuditLogs/{server.Id}");
    }

    private void OpenRolesPage()
    {
        navManager.NavigateTo($"/ServerSettings/Roles/{server.Id}");
    }

    private void OpenCreateRolePage()
    {
        navManager.NavigateTo($"/ServerSettings/CreateRole/{server.Id}");
    }

    private void OpenCategoriesPage()
    {
        navManager.NavigateTo($"/ServerSettings/Categories/{server.Id}");
    }

    private void OpenCreateCategoryPage()
    {
        navManager.NavigateTo($"/ServerSettings/CreateCategory/{server.Id}");
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