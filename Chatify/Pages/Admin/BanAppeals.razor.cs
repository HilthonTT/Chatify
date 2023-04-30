using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.Admin;

public partial class BanAppeals
{
    private UserModel loggedInUser;
    private List<BanAppealModel> appeals;
    private List<UserModel> users;
    private string searchUserText = "";
    private string searchAppealText = "";
    private string selectedUsername = "";
    private bool isSortedByNew = true;
    private bool showUsernames = false;
    private bool filterByApproved = false;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        appeals = await appealData.GetAllBanAppealsAsync();
        users = await userData.GetAllUsersCachedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterAppeals();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchAppealText));
        searchAppealText = stringResults.Success ? stringResults.Value : "";
        stringResults = await sessionStorage.GetAsync<string>(nameof(selectedUsername));
        selectedUsername = stringResults.Success ? stringResults.Value : "All";
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
        boolResults = await sessionStorage.GetAsync<bool>(nameof(filterByApproved));
        filterByApproved = boolResults.Success ? boolResults.Value : false;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(searchAppealText), searchAppealText);
        await sessionStorage.SetAsync(nameof(selectedUsername), selectedUsername);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        await sessionStorage.SetAsync(nameof(filterByApproved), filterByApproved);
    }

    private async Task FilterAppeals()
    {
        var output = await appealData.GetAllBanAppealsAsync();
        if (selectedUsername != "All")
        {
            output = output.Where(a => a.AppealingUser?.DisplayName == selectedUsername).ToList();
        }

        if (string.IsNullOrWhiteSpace(searchAppealText)is false)
        {
            output = output.Where(a => a.AppealReason.Contains(searchAppealText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (filterByApproved)
        {
            output = output.Where(a => a.IsApproved).ToList();
        }
        else
        {
            output = output.Where(a => a.IsApproved is false).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(a => a.SubmittedAt).ToList();
        }
        else
        {
            output = output.OrderBy(a => a.SubmittedAt).ToList();
        }

        appeals = output;
        await SaveFilterState();
    }

    private async Task FilterUsers()
    {
        var output = await userData.GetAllUsersCachedAsync();
        if (string.IsNullOrWhiteSpace(searchUserText)is false)
        {
            output = output.Where(u => u.DisplayName.Contains(searchUserText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        users = output;
        await SaveFilterState();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterAppeals();
    }

    private async Task OnAppealSearchInput(string searchInput)
    {
        searchAppealText = searchInput;
        await FilterAppeals();
    }

    private async Task OnUserSearchInput(string searchInput)
    {
        searchUserText = searchInput;
        await FilterUsers();
    }

    private async Task OnUsernameClick(string username = "All")
    {
        selectedUsername = username;
        showUsernames = false;
        await FilterAppeals();
    }

    private async Task FilterByApprovedClick(bool isApproved)
    {
        filterByApproved = isApproved;
        await FilterAppeals();
    }

    private void OpenDetails(BanAppealModel appeal)
    {
        navManager.NavigateTo($"/BanAppealDetails/{appeal.Id}");
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isNew == isSortedByNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private string FilterByApprovedClass(bool isApproved)
    {
        if (isApproved == filterByApproved)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private string SelectedUsernameClass(string username = "All")
    {
        if (username == selectedUsername)
        {
            return "bg-secondary";
        }

        return "bg-dark";
    }
}