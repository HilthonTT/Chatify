using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;

namespace Chatify.Pages.Servers;

public partial class Servers
{
    private UserModel loggedInUser;
    private List<ServerModel> servers;
    private List<CategoryModel> categories;
    private BanModel ban;
    private string searchText = "";
    private string selectedCategory = "";
    private bool isSortedByNew = false;
    private bool showCategories = false;
    private bool showFriends = false;
    private bool showServersThatUserIsIn = false;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        categories = await categoryData.GetAllCategoriesAsync();
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
            await FilterServers();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
        stringResults = await sessionStorage.GetAsync<string>(nameof(selectedCategory));
        selectedCategory = stringResults.Success ? stringResults.Value : "All";
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
        boolResults = await sessionStorage.GetAsync<bool>(nameof(showServersThatUserIsIn));
        showServersThatUserIsIn = boolResults.Success ? boolResults.Value : true;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(searchText), searchText);
        await sessionStorage.SetAsync(nameof(selectedCategory), selectedCategory);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        await sessionStorage.SetAsync(nameof(showServersThatUserIsIn), showServersThatUserIsIn);
    }

    private async Task FilterServers()
    {
        var output = await serverData.GetAllServersAsync();
        if (selectedCategory != "All")
        {
            output = output.Where(s => s.Category?.CategoryName == selectedCategory).ToList();
        }

        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(s => s.ServerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || s.Owner.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(s => s.DateCreated).ToList();
        }
        else
        {
            output = output.OrderByDescending(s => s.Members.Count).ThenByDescending(s => s.DateCreated).ToList();
        }

        servers = output;
        await SaveFilterState();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterServers();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterServers();
    }

    private async Task OnCategoryClick(string category = "All")
    {
        selectedCategory = category;
        showCategories = false;
        await FilterServers();
    }

    private async Task OpenPrivateConversation(BasicUserModel user)
    {
        var conversation = await privateConversationData.GetUsersConversationAsync(loggedInUser.Id, user.Id);
        if (conversation is null)
        {
            string objectId = await oidGenerator.GenerateOidAsync();
            PrivateConversationModel c = new()
            {
                ObjectIdentifier = objectId,
                FirstParticipant = new BasicUserModel(loggedInUser),
                LastParticipant = user,
            };
            await privateConversationData.CreateConversation(c);
            conversation = await privateConversationData.GetConversationObjectIdAsync(objectId);
            navManager.NavigateTo($"/PrivateConversation/{conversation.Id}");
        }
        else
        {
            navManager.NavigateTo($"/PrivateConversation/{conversation.Id}");
        }
    }

    private void LoadUsersPage()
    {
        navManager.NavigateTo("/Users");
    }

    private void LoadCreatePage()
    {
        if (loggedInUser is not null)
        {
            navManager.NavigateTo("/CreateServer");
        }
        else
        {
            navManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isNew == isSortedByNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private string SelectedCategoryClass(string category = "All")
    {
        if (category == selectedCategory)
        {
            return "bg-secondary";
        }

        return "bg-dark";
    }

    private string SelectedCategoryIcon(string category = "All")
    {
        if (category == "All")
        {
            return "oi-book";
        }

        if (category == "Gaming")
        {
            return "oi-bug";
        }

        if (category == "Music")
        {
            return "oi-musical-note";
        }

        if (category == "Sports")
        {
            return "oi-badge";
        }

        if (category == "Sciences")
        {
            return "oi-beaker";
        }

        if (category == "Technology")
        {
            return "oi-monitor";
        }

        return "oi-question-mark";
    }
}