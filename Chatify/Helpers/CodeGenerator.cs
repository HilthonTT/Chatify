using ChatifyLibrary.DataAccess;

namespace Chatify.Helpers;

public class CodeGenerator : ICodeGenerator
{
    private readonly IUserData _userData;
    private readonly IServerData _serverData;
    private readonly IMessageData _messageData;

    public CodeGenerator(IUserData userData,
                         IServerData serverData,
                         IMessageData messageData)
    {
        _userData = userData;
        _serverData = serverData;
        _messageData = messageData;
    }

    public async Task<string> GenerateFriendCodeAsync()
    {
        string friendCode = null;
        var users = await _userData.GetAllUsersAsync();

        while (friendCode is null)
        {
            friendCode = GenerateRandomString();

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
            invitationCode = GenerateRandomString();

            var existingServer = servers.Where(s => s.InvitationCode == invitationCode).FirstOrDefault();

            if (existingServer is not null)
            {
                // Invitation code is already in use, generate a new one.
                invitationCode = null;
            }
        }

        return invitationCode;
    }

    public async Task<string> GenerateRandomMessageIdentifier()
    {
        string objectIdentifier = null;
        var messages = await _messageData.GetAllMessagesAsync();

        while (objectIdentifier is null)
        {
            objectIdentifier = GenerateRandomString();

            var existingMessage = messages.Where(s => s.ObjectIdentifier == objectIdentifier).FirstOrDefault();

            if (existingMessage is not null)
            {
                // Object Identifier is already in use, generate a new one.
                objectIdentifier = null;
            }
        }

        return objectIdentifier;
    }

    private static string GenerateRandomString()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
