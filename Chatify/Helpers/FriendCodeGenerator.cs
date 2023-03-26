using ChatifyLibrary.DataAccess;

namespace Chatify.Helpers;

public class FriendCodeGenerator : IFriendCodeGenerator
{
    private readonly IUserData _userData;

    public FriendCodeGenerator(IUserData userData)
    {
        _userData = userData;
    }

    public async Task<string> GenerateFriendCodeAsync()
    {
        string friendCode = null;
        var users = await _userData.GetAllUsersAsync();

        while (friendCode is null)
        {
            friendCode = GenerateRandomFriendCode();

            var existingUser = users.Where(u => u.FriendCode == friendCode).FirstOrDefault();

            if (existingUser != null)
            {
                // Friend code is already in use, generate a new one
                friendCode = null;
            }
        }

        return friendCode;
    }

    private string GenerateRandomFriendCode()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
