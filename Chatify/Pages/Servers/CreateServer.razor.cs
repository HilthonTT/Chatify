using Microsoft.AspNetCore.Components.Forms;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Servers;

public partial class CreateServer
{
    private CreateServerModel server = new();
    private List<CategoryModel> categories;
    private UserModel loggedInUser;
    private BanModel ban;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        categories = await categoryData.GetAllCategoriesAsync();
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    private void OpenServerDetails(ServerModel server)
    {
        navManager.NavigateTo($"/Server/{server.Id}");
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/Servers");
    }

    private async Task Create()
    {
        string relativePath = await CaptureFile();
        string objectId = await oidGenerator.GenerateOidAsync();
        ServerModel s = new()
        {
            ObjectIdentifier = objectId,
            Owner = new BasicUserModel(loggedInUser),
            Category = categories.Where(c => c.Id == server.CategoryId).FirstOrDefault(),
            ServerName = server.ServerName,
            ServerDescription = server.ServerDescription,
            PictureName = relativePath,
            OriginalPictureName = fileName,
            PictureExtension = fileExtension,
            Archived = false,
        };
        s.Members.Add(new BasicUserModel(loggedInUser));
        if (s.Category is null)
        {
            server.CategoryId = "";
            return;
        }

        var newServer = await serverData.CreateServerAndReturn(s);
        RoleModel r = new()
        {
            RoleName = "Member",
            RoleDescription = "Default role, anyone who joins this server has this role.",
            Server = newServer,
        };
        r.Users.Add(new BasicUserModel(loggedInUser));
        var newRole = await roleData.CreateRoleAndReturn(r);
        string channelObjectId = await oidGenerator.GenerateOidAsync();
        ChannelModel c = new()
        {
            ObjectIdentifier = channelObjectId,
            ChannelName = "Default Channel",
            ChannelDescription = "This is the default channel.",
            Server = newServer,
        };
        c.AllowedRoles.Add(newRole);
        await channelData.CreateChannel(c);
        string categoryObjectId = await oidGenerator.GenerateOidAsync();
        ChannelCategoryModel ca = new()
        {
            ObjectIdentifier = categoryObjectId,
            CategoryName = "Default Category",
            CategoryDescription = "Default Category Description",
            Server = new BasicServerModel(newServer),
        };
        ca.Channels.Add(c);
        await channelCategoryData.CreateCategory(ca);
        file = null;
        fileName = "";
        fileExtension = "";
        server = new();
        OpenServerDetails(newServer);
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
}