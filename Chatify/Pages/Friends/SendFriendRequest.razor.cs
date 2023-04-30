using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Friends;

public partial class SendFriendRequest
{
    private CreateFriendRequestModel request = new();
    private List<FriendRequestModel> friendRequests;
    private UserModel loggedInUser;
    private BanModel ban;
    private string errorMessage = "";
    private bool isSuccess = false;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            friendRequests = await requestData.GetUserSendedFriendRequestsAsync(loggedInUser.Id);
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    private async Task SendRequest()
    {
        isSuccess = false;
        errorMessage = "";
        var user = await userData.GetUserFriendCodeAsync(request.FriendCode);
        var friendRequest = friendRequests.Where(f => f.Receiver.Id == user.Id).FirstOrDefault();
        string objectId = await oidGenerator.GenerateOidAsync();
        if (user is null)
        {
            errorMessage = "Oops... no user have been founded having this friend code.";
            return;
        }

        if (loggedInUser.Id == user.Id)
        {
            errorMessage = "You can't send a friend request to yourself.";
            return;
        }

        if (loggedInUser.Friends.Contains(new BasicUserModel(user)))
        {
            errorMessage = "You already have this person in your friend list.";
            return;
        }

        if (friendRequest is not null)
        {
            errorMessage = "You already have sended a friend request to this person.";
            return;
        }

        FriendRequestModel r = new()
        {
            ObjectIdentifier = objectId,
            Sender = new BasicUserModel(loggedInUser),
            Receiver = new BasicUserModel(user),
        };
        await requestData.CreateFriendRequest(r);
        friendRequests.Add(r);
        request = new();
        errorMessage = "The friend request have been sended";
        isSuccess = true;
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private string GetErrorMessageClass()
    {
        if (isSuccess)
        {
            return "text-success";
        }

        return "text-danger";
    }
}