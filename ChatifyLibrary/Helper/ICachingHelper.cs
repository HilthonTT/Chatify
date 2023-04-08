namespace ChatifyLibrary.Helper;

public interface ICachingHelper
{
    string BanAppealCachingString(string id);
    string BanCachingString(string id);
    string ConversationCachingString(string id);
    string SendedFriendRequestCachingString(string id);
    string MessageCachingString(string id);
    string PrivateConversationCachingString(string firstId, string secondId);
    string PrivateMessageCachingString(string id);
    string ReceivedFriendRequestCachingString(string id);
}