namespace Chatify.Helpers;

public interface ICodeGenerator
{
    Task<string> GenerateFriendCodeAsync();
    Task<string> GenerateConversationIdentifierAsync();
    Task<string> GenerateMessageIdentifierAsync();
    Task<string> GenerateServerIdentifierAsync();
    Task<string> GenerateServerInvationCodeAsync();
    Task<string> GenerateChannelIdentifierAsync();
    Task<string> GeneratePrivateMessageIdentifierAsync();
    Task<string> GeneratePrivateConversationIdentifierAsync();
    Task<string> GenerateFriendRequestIdentifierAsync();
    Task<string> GenerateServerInvitationIdentifierAsync();
}