using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Conversations;

public partial class Edit
{
    [Parameter]
    public string Id { get; set; }

    private CreateConversationModel editingConversation = new();
    private ConversationModel conversation;
    private List<CategoryModel> categories;
    private ConversationModel selectedConversation;
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
        conversation = await conversationData.GetConversationAsync(Id);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }

        if (conversation is not null)
        {
            editingConversation.GroupName = conversation.GroupName;
            editingConversation.CategoryId = conversation.Category.Id;
            editingConversation.PictureName = conversation.PictureName;
        }
    }

    private async Task CloseModal(string id)
    {
        await JSRuntime.InvokeVoidAsync("closeModal", id);
    }

    private async Task UpdateConversation()
    {
        string relativePath = await CaptureFile();
        string groupName = conversation.GroupName;
        string pictureName = conversation.PictureName;
        string originalPictureName = conversation.OriginalPictureName;
        string pictureExtension = conversation.PictureExtension;
        conversation.GroupName = editingConversation.GroupName;
        conversation.Category = categories.Where(c => c.Id == editingConversation.CategoryId).FirstOrDefault();
        if (file is not null)
        {
            conversation.PictureName = relativePath;
            conversation.OriginalPictureName = fileName;
            conversation.PictureExtension = fileExtension;
        }
        else
        {
            conversation.PictureName = pictureName;
            conversation.OriginalPictureName = originalPictureName;
            conversation.PictureExtension = pictureExtension;
        }

        AuditLogModel a = new()
        {
            Name = "Conversation Details Updated",
            Description = $"{loggedInUser.DisplayName} has updated conversation of ID {conversation.Id}'s details",
            OldValues = $@"Conversation name: {conversation.GroupName} 
                Conversation Picture: {conversation.PictureName}
                Original picture name: {conversation.OriginalPictureName}",
            NewValues = $@"Conversation name: {groupName} 
                Conversation Picture: {pictureName} 
                Original picture name: {originalPictureName}",
            User = new BasicUserModel(loggedInUser),
            Conversation = new BasicConversationModel(conversation),
            DateModified = DateTime.UtcNow,
        };
        await conversationData.UpdateConversation(conversation);
        await auditLogData.CreateAuditLog(a);
        file = null;
        await CloseModal("conversationModal");
    }

    private async Task ArchiveConversation()
    {
        conversation.Archived = true;
        await conversationData.UpdateConversation(conversation);
        await CloseModal("conversationModal");
        ClosePage();
    }

    private void ClearPicture()
    {
        conversation.PictureName = "";
        conversation.OriginalPictureName = "";
        conversation.PictureExtension = "";
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void ReturnToConversation(ConversationModel conversation)
    {
        navManager.NavigateTo($"/Conversation/{conversation.Id}");
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
}