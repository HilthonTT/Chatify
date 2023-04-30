using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;

namespace Chatify.Components;

public partial class FriendRequestComponent
{
    [Parameter]
    [EditorRequired]
    public FriendRequestModel FriendRequest { get; set; }

    [Parameter]
    [EditorRequired]
    public List<FriendRequestModel> PendingRequests { get; set; }

    [Parameter]
    [EditorRequired]
    public UserModel LoggedInUser { get; set; }

    [Parameter]
    public EventCallback<FriendRequestModel> RequestChanged { get; set; }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", $"requestModal-{FriendRequest.Id}");
    }

    private async Task AcceptRequest()
    {
        if (FriendRequest.IsAccepted)
            return;
        var sender = await userData.GetUserAsync(FriendRequest.Sender.Id);
        FriendRequest.IsAccepted = true;
        LoggedInUser.Friends.Add(FriendRequest.Sender);
        sender.Friends.Add(new BasicUserModel(LoggedInUser));
        await requestData.UpdateFriendRequest(FriendRequest);
        await userData.UpdateUser(LoggedInUser);
        await userData.UpdateUser(sender);
        PendingRequests.Remove(FriendRequest);
        await RequestChanged.InvokeAsync(FriendRequest);
        await CloseModal();
    }

    private string GetStatusText()
    {
        if (FriendRequest.IsAccepted)
        {
            return "Accepted";
        }

        return "Not Considered";
    }
}