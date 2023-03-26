namespace Chatify.Helpers
{
    public interface IFriendCodeGenerator
    {
        Task<string> GenerateFriendCodeAsync();
    }
}