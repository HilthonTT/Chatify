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
    string ChannelCachingString(string id);
    string ServerCachingString(string id);
    string ServerInvitationCachingString(string id);
    string UserCachingString(string id);
    string RoleCachingString(string id);
    string AuditLogCachingString(string id);
    string ChannelCategoryCachingString(string id);
}