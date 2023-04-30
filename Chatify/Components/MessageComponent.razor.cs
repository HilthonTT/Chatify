using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Models;

namespace Chatify.Components;

public partial class MessageComponent
{
    [Parameter]
    [EditorRequired]
    public MessageModel Message { get; set; }

    [Parameter]
    [EditorRequired]
    public UserModel LoggedInUser { get; set; }

    [Parameter]
    [EditorRequired]
    public List<MessageModel> NonReadMessages { get; set; }

    [Parameter]
    [EditorRequired]
    public List<MessageModel> Messages { get; set; }

    [Parameter]
    public EventCallback<MessageModel> RequestChanged { get; set; }

    private CreateMessageModel model = new();
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string errorMessage = "";
    private string fileName = "";
    private string fileExtension = "";
    protected override void OnInitialized()
    {
        model.Text = Message.Text;
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
        errorMessage = "";
        if (file is null || LoggedInUser is null)
            return "";
        try
        {
            string newFileName = Path.ChangeExtension(Path.GetRandomFileName(), Path.GetExtension(file.Name));
            string path = Path.Combine(config.GetValue<string>("FileStorage"), LoggedInUser.Email, newFileName);
            string relativePath = Path.Combine(LoggedInUser.Email, newFileName);
            Directory.CreateDirectory(Path.Combine(config.GetValue<string>("FileStorage"), LoggedInUser.Email));
            await using FileStream fs = new(path, FileMode.Create);
            await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
            return relativePath;
        }
        catch (Exception ex)
        {
            errorMessage = $"File: {file.Name} Error: {ex.Message}";
            throw;
        }
    }

    private async Task CloseModal(MessageModel message)
    {
        await JSRuntime.InvokeVoidAsync("closeModal", $"messageModal-{message.Id}");
    }

    private async Task UpdateMessage()
    {
        string relativePath = await CaptureFile();
        string oldFileName = Message.FileName;
        string oldOriginalFile = Message.OriginalFileName;
        string oldExtension = Message.FileExtension;
        Message.Text = model.Text;
        Message.LastEdited = DateTime.UtcNow;
        if (file is not null)
        {
            Message.FileName = relativePath;
            Message.OriginalFileName = fileName;
            Message.FileExtension = fileExtension;
        }
        else
        {
            Message.FileName = oldFileName;
            Message.OriginalFileName = oldOriginalFile;
            Message.FileExtension = oldExtension;
        }

        await messageData.UpdateMessageAsync(Message);
        model = new();
        file = null;
        await RequestChanged.InvokeAsync(Message);
        await CloseModal(Message);
    }

    private async Task MarkAsRead()
    {
        Message.ReadBy.Add(new BasicUserModel(LoggedInUser));
        await messageData.UpdateMessageAsync(Message);
        NonReadMessages.Remove(Message);
        await RequestChanged.InvokeAsync(Message);
    }

    private async Task ArchiveMessage()
    {
        Message.Archived = true;
        await messageData.UpdateMessageAsync(Message);
        Messages.Remove(Message);
        await RequestChanged.InvokeAsync(Message);
        await CloseModal(Message);
    }

    private async Task JoinServer(ServerModel server)
    {
        var user = new BasicUserModel(LoggedInUser);
        var role = await roleData.GetServerMemberRoleAsync(server);
        role.Users.Add(user);
        var serverRole = await roleData.GetServerMemberRoleAsync(server);
        serverRole.Users.Add(user);
        await roleData.UpdateRole(role);
        await roleData.UpdateRole(serverRole);
        server.Members.Add(user);
        await serverData.UpdateServer(server);
        OpenDetails(server);
    }

    private void OpenDetails(ServerModel server)
    {
        navManager.NavigateTo($"/Server/{server.Id}");
    }

    private bool UserHasReadMessage()
    {
        var user = Message.ReadBy.FirstOrDefault(u => u.Id == LoggedInUser.Id);
        if (user is not null)
        {
            return true;
        }

        return false;
    }

    private bool IsUserBanned(ServerModel server)
    {
        bool IsUserInBannedList = server.BannedUsers.Any(b => b.Id == LoggedInUser?.Id);
        if (IsUserInBannedList)
        {
            return true;
        }

        return false;
    }

    private bool IsUserInServer(ServerModel server)
    {
        if (server.Owner.Id == LoggedInUser?.Id || server.Members.Any(m => m.Id == LoggedInUser?.Id))
        {
            return true;
        }

        return false;
    }
}