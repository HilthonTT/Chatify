namespace Chatify.Helpers
{
    public interface ICodeGenerator
    {
        Task<string> GenerateFriendCodeAsync();
        Task<string> GenerateRandomMessageIdentifier();
        Task<string> GenerateServerInvationCodeAsync();
    }
}