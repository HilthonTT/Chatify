using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;

namespace Chatify.Components;

public partial class ConversationComponent
{
    [Parameter]
    [EditorRequired]
    public ConversationModel Conversation { get; set; }

    [Parameter]
    [EditorRequired]
    public UserModel LoggedInUser { get; set; }

    private List<MessageModel> nonReadMessages;
    protected override async Task OnInitializedAsync()
    {
        nonReadMessages = await messageData.GetConversationUnreadMessagesAsync(Conversation, LoggedInUser);
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private void OpenDetails(ConversationModel conversation)
    {
        navManager.NavigateTo($"/Conversation/{conversation.Id}");
    }

    private bool CanUserCheckOut()
    {
        if (Conversation.Participants.Select(p => p.Id == LoggedInUser.Id)is not null || Conversation.Owner.Id == LoggedInUser.Id)
        {
            return true;
        }

        return false;
    }

    private string GetNonReadMessagesCount()
    {
        if (nonReadMessages?.Count == 0)
        {
            return "";
        }

        if (nonReadMessages?.Count == 1)
        {
            return "1 unread message";
        }

        if (nonReadMessages?.Count > 99)
        {
            return "99+ unread messages";
        }

        if (nonReadMessages?.Count < 99)
        {
            return $"{nonReadMessages.Count} unread messages";
        }

        return "";
    }
}