using ChatifyLibrary.DataAccess;

namespace Chatify.Helpers;

public class CodeGenerator : ICodeGenerator
{
    private readonly IUserData _userData;
    private readonly IServerData _serverData;

    public CodeGenerator(IUserData userData,
                         IServerData serverData)
    {
        _userData = userData;
        _serverData = serverData;
    }

    public async Task<string> GenerateFriendCodeAsync()
    {
        string friendCode = null;
        var users = await _userData.GetAllUsersAsync();

        while (friendCode is null)
        {
            friendCode = GenerateRandomFriendCode();

            var existingUser = users.Where(u => u.FriendCode == friendCode).FirstOrDefault();

            if (existingUser is not null)
            {
                // Friend code is already in use, generate a new one
                friendCode = null;
            }
        }

        return friendCode;
    }

    public async Task<string> GenerateServerInvationCodeAsync()
    {
        string invitationCode = null;
        var servers = await _serverData.GetAllServersAsync();

        while (invitationCode is null)
        {
            invitationCode = GenerateRandomServerInvitationCode();

            var existingServer = servers.Where(s => s.InvitationCode == invitationCode).FirstOrDefault();

            if (existingServer is not null)
            {
                // Invitation code is already in use, generate a new one.
                invitationCode = null;
            }
        }

        return invitationCode;
    }

    private static string GenerateRandomFriendCode()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GenerateRandomServerInvitationCode()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
