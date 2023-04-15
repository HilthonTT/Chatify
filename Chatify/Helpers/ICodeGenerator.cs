namespace Chatify.Helpers;

public interface ICodeGenerator
{
    Task<string> GenerateFriendCodeAsync();
    Task<string> GenerateConversationIdentifier();
    Task<string> GenerateMessageIdentifier();
    Task<string> GenerateServerIdentifierAsync();
    Task<string> GenerateServerInvationCodeAsync();
    Task<string> GenerateChannelIdentifier();
    Task<string> GeneratePrivateMessageIdentifier();
    Task<string> GeneratePrivateConversationIdentifier();
    Task<string> GenerateFriendRequestIdentifier();
    Task<string> GenerateServerInvitationIdentifier();
}