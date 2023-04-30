using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;

namespace Chatify.Pages.User;

public partial class UserDetails
{
    [Parameter]
    public string Id { get; set; }

    private UserModel loggedInUser;
    private UserModel user;
    private List<ConversationModel> conversations;
    private BanModel ban;
    private string errorMessage = "";
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        user = await userData.GetUserAsync(Id);
        if (loggedInUser is not null)
        {
            conversations = await conversationData.GetUserConversationsAsync(user.Id);
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    private async Task CloseModal(string id)
    {
        await JSRuntime.InvokeVoidAsync("closeModal", id);
    }

    private async Task OpenPrivateConversation(UserModel user)
    {
        var conversation = await privateConversationData.GetUsersConversationAsync(loggedInUser.Id, user.Id);
        if (conversation is null)
        {
            PrivateConversationModel c = new()
            {
                FirstParticipant = new BasicUserModel(loggedInUser),
                LastParticipant = new BasicUserModel(user),
            };
            await privateConversationData.CreateConversation(c);
            conversation = await privateConversationData.GetUsersConversationAsync(loggedInUser.Id, user.Id);
            navManager.NavigateTo($"/PrivateConversation/{conversation.Id}");
        }
        else
        {
            navManager.NavigateTo($"/PrivateConversation/{conversation.Id}");
        }
    }

    private async Task SendFriendRequest()
    {
        errorMessage = "";
        string objectId = await oidGenerator.GenerateOidAsync();
        if (loggedInUser is null)
            return;
        var model = await userData.GetUserFriendCodeAsync(user.FriendCode);
        var friendRequest = await requestData.GetAlreadySendedFriendRequestAsync(loggedInUser, user);
        if (loggedInUser.Id == model.Id)
        {
            errorMessage = "You can't send a friend request to yourself.";
            return;
        }

        if (model is null)
        {
            errorMessage = "Oops... no user have been founded having this friend code.";
            return;
        }

        if (friendRequest is not null)
        {
            errorMessage = "You have already sent a friend request or have this person in your friend list.";
            return;
        }

        var newFriendRequest = new FriendRequestModel
        {
            ObjectIdentifier = objectId,
            Sender = new BasicUserModel(loggedInUser),
            Receiver = new BasicUserModel(model),
        };
        await requestData.CreateFriendRequest(newFriendRequest);
        await CloseModal("friendModal");
    }

    private async Task UnfriendUser()
    {
        errorMessage = "";
        if (loggedInUser is null)
            return;
        var friendRequest = await requestData.GetSenderAndReceiverFriendRequestAsync(loggedInUser, user);
        var friendToRemove = user.Friends.FirstOrDefault(f => f.Id == loggedInUser.Id);
        if (friendToRemove is not null)
        {
            user.Friends.Remove(friendToRemove);
        }

        var loggedInUserFriendToRemove = loggedInUser.Friends.FirstOrDefault(f => f.Id == user.Id);
        if (loggedInUserFriendToRemove is not null)
        {
            loggedInUser.Friends.Remove(loggedInUserFriendToRemove);
        }

        if (friendRequest is not null)
        {
            await requestData.DeleteFriendRequestAsync(friendRequest);
        }

        await userData.UpdateUser(user);
        await userData.UpdateUser(loggedInUser);
        await CloseModal("unfriendModal");
    }

    private async Task ToggleBlockUser()
    {
        var userToBlock = loggedInUser.BlockedUsers.FirstOrDefault(u => u.Id == user.Id);
        if (userToBlock is not null)
        {
            // User is already blocked, so unblock them
            loggedInUser.BlockedUsers.Remove(userToBlock);
            await UnfriendUser();
        }
        else
        {
            // User is not blocked, so block them
            await UnfriendUser();
            loggedInUser.BlockedUsers.Add(new BasicUserModel(user));
        }

        await userData.UpdateUser(loggedInUser);
        await CloseModal("blockModal");
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }

    private void OpenAdminBan(UserModel user)
    {
        navManager.NavigateTo($"/AdminBan/{user.Id}");
    }

    private void OpenDetails(BasicUserModel user)
    {
        if (loggedInUser?.Id == user.Id)
        {
            navManager.NavigateTo("/Profile");
            return;
        }

        navManager.NavigateTo($"/UserDetails/{user.Id}");
    }

    private string BlockButtonText()
    {
        var userToBlock = loggedInUser?.BlockedUsers.FirstOrDefault(u => u.Id == user.Id);
        if (userToBlock is not null)
        {
            return "Unblock";
        }

        return "Block";
    }

    private bool IsUserFriends()
    {
        var loggedInUserFriend = loggedInUser.Friends.FirstOrDefault(u => u.Id == user.Id);
        if (loggedInUserFriend is null)
        {
            return false;
        }

        return true;
    }

    private bool UserHasBlockedLoggedInUser()
    {
        var blockedLoggedInUser = user?.BlockedUsers.FirstOrDefault(u => u.Id == loggedInUser?.Id);
        if (blockedLoggedInUser is null)
        {
            return false;
        }

        return true;
    }

    private bool LoggedInUserHasBlockedUser()
    {
        var userBlockedInLoggedInUser = loggedInUser?.BlockedUsers.FirstOrDefault(u => u.Id == loggedInUser?.Id);
        if (userBlockedInLoggedInUser is null)
        {
            return false;
        }

        return true;
    }

    private bool HasAnyoneBlockedAnyone()
    {
        var userBlockedLoggedInUser = UserHasBlockedLoggedInUser();
        var loggedInUserBlockedUser = LoggedInUserHasBlockedUser();
        return userBlockedLoggedInUser || loggedInUserBlockedUser;
    }
}