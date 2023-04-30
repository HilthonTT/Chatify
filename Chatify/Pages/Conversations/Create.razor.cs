using Microsoft.AspNetCore.Components.Forms;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Conversations;

public partial class Create
{
    private CreateConversationModel conversation = new();
    private List<CategoryModel> categories;
    private List<BasicUserModel> friends;
    private UserModel loggedInUser;
    private BasicUserModel selectedFriend;
    private BanModel ban;
    private string searchText = "";
    private string errorMessage = "";
    private long maxFileSize = 1024 * 1024 * 3; // represents 3MB
    private IBrowserFile? file;
    private string fileName = "";
    private string fileExtension = "";
    protected override async Task OnInitializedAsync()
    {
        categories = await categoryData.GetAllCategoriesAsync();
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        friends = loggedInUser.Friends;
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterFriends();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterFriends()
    {
        var output = new List<BasicUserModel>();
        if (loggedInUser is not null)
        {
            output = loggedInUser.Friends;
        }

        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(f => f.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        friends = output;
        await SaveFilterState();
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void GoToConversation(ConversationModel conversation)
    {
        navManager.NavigateTo($"/conversation/{conversation.Id}");
    }

    private void AddParticipant(BasicUserModel participant)
    {
        conversation.Participants.Add(participant);
        loggedInUser.Friends.Remove(participant);
        selectedFriend = null;
    }

    private void RemoveParticipant(BasicUserModel participant)
    {
        conversation.Participants.Remove(participant);
        loggedInUser.Friends.Add(participant);
        selectedFriend = null;
    }

    private async Task CreateConversation()
    {
        string relativePath = await CaptureFile();
        string objectIdentifier = await oidGenerator.GenerateOidAsync();
        ConversationModel c = new()
        {
            ObjectIdentifier = objectIdentifier,
            Owner = new BasicUserModel(loggedInUser),
            Participants = conversation.Participants,
            Category = categories.Where(c => c.Id == conversation.CategoryId).FirstOrDefault(),
            PictureName = relativePath,
            OriginalPictureName = fileName,
            PictureExtension = fileExtension,
            GroupName = conversation.GroupName,
            IsGroupChat = true,
        };
        c.Participants.Add(new BasicUserModel(loggedInUser));
        if (c.Category is null)
        {
            conversation.CategoryId = "";
            return;
        }

        if (c.Participants.Count > 10)
        {
            errorMessage = "You cannot have more than 10 participants in a conversation.\n Try making a server instead.";
            return;
        }

        file = null;
        fileName = string.Empty;
        fileExtension = string.Empty;
        await conversationData.CreateConversation(c);
        var newConversation = await conversationData.GetConversationByObjectIdentifier(objectIdentifier);
        conversation = new();
        GoToConversation(newConversation);
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterFriends();
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