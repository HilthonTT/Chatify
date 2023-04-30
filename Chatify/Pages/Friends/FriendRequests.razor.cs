using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.Friends;

public partial class FriendRequests
{
    [CascadingParameter]
    public EventCallback<FriendRequestModel> RequestChanged { get; set; }

    private UserModel loggedInUser;
    private List<FriendRequestModel> pendingRequests;
    private BanModel ban;
    private string searchText = "";
    private bool isSortedByNew = true;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            pendingRequests = await requestData.GetUserPendingFriendRequestsAsync(loggedInUser.Id);
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterPendingRequests();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(searchText), searchText);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
    }

    private async Task FilterPendingRequests()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        var output = await requestData.GetUserPendingFriendRequestsAsync(loggedInUser.Id);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(f => f.Receiver.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || f.Sender.DisplayName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(f => f.RequestDate).ToList();
        }
        else
        {
            output = output.OrderBy(f => f.RequestDate).ToList();
        }

        pendingRequests = output;
        await SaveFilterState();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterPendingRequests();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterPendingRequests();
    }

    private string GetFriendRequestCount()
    {
        if (pendingRequests?.Count > 1)
        {
            return "Friend Requests";
        }

        return "Friend Request";
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isSortedByNew == isNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }
}