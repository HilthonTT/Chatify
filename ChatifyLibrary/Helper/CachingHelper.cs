namespace ChatifyLibrary.Helper;

public class CachingHelper : ICachingHelper
{
    public string BanAppealCachingString(string id)
    {
        return $"BanAppealData-{id}";
    }

    public string BanCachingString(string id)
    {
        return $"BanData-{id}";
    }

    public string ConversationCachingString(string id)
    {
        return $"ConversationData-{id}";
    }

    public string SendedFriendRequestCachingString(string id)
    {
        return $"SendedFriendRequestData-{id}";
    }

    public string ReceivedFriendRequestCachingString(string id)
    {
        return $"ReceivedFriendRequestData-{id}";
    }

    public string MessageCachingString(string id)
    {
        return $"MessageData-{id}";
    }

    public string PrivateConversationCachingString(string firstId, string secondId)
    {
        return $"PrivateConversationData-{firstId}-{secondId}";
    }

    public string PrivateMessageCachingString(string id)
    {
        return $"PrivateMessageData-{id}";
    }

    public string ChannelCachingString(string id)
    {
        return $"ChannelData-{id}";
    }

    public string ServerCachingString(string id)
    {
        return $"ServerData-{id}";
    }

    public string ServerInvitationCachingString(string id)
    {
        return $"InvitationData-{id}";
    }
}
