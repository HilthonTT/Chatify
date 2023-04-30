using Microsoft.AspNetCore.Components.Forms;
using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.User;

public partial class Profile
{
    private UserModel loggedInUser;
    private List<ConversationModel> conversations;
    private BanModel ban;
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            conversations = await conversationData.GetUserConversationsAsync(loggedInUser.Id);
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private async Task UpdateProfilePicture()
    {
        string relativePath = await CaptureFile();
        loggedInUser.FileName = relativePath;
        await userData.UpdateUser(loggedInUser);
        file = null;
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void EditProfilePage()
    {
        navManager.NavigateTo("/MicrosoftIdentity/Account/EditProfile", true);
    }

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        file = e.File;
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