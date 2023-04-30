using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Shared;

public partial class NavMenu
{
    private string Username => loggedInUser is not null ? loggedInUser.DisplayName : "???";
    private string searchText = "";
    private UserModel loggedInUser;
    private List<FriendRequestModel> friendRequests;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            friendRequests = await requestData.GetUserPendingFriendRequestsAsync(loggedInUser.Id);
        }
    }

    private void GoToUsers()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            navManager.NavigateTo($"/Users");
        }

        navManager.NavigateTo($"/Users/{searchText}", true);
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }
}