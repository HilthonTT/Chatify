namespace Chatify.Helpers;

public interface ICodeGenerator
{
    Task<string> GenerateFriendCodeAsync();
    Task<string> GenerateConversationIdentifierAsync();
    Task<string> GenerateMessageIdentifierAsync();
    Task<string> GenerateServerIdentifierAsync();
    Task<string> GenerateChannelIdentifierAsync();
    Task<string> GeneratePrivateConversationIdentifierAsync();
    Task<string> GenerateFriendRequestIdentifierAsync();
    Task<string> GenerateChannelCategoryIdentifierAsync();
}