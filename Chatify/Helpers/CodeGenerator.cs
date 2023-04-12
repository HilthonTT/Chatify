using ChatifyLibrary.DataAccess;

namespace Chatify.Helpers;

public class CodeGenerator : ICodeGenerator
{
    private readonly IUserData _userData;
    private readonly IServerData _serverData;
    private readonly IMessageData _messageData;
    private readonly IConversationData _conversationData;

    public CodeGenerator(IUserData userData,
                         IServerData serverData,
                         IMessageData messageData,
                         IConversationData conversationData)
    {
        _userData = userData;
        _serverData = serverData;
        _messageData = messageData;
        _conversationData = conversationData;
    }

    private static async Task<string> GenerateCodeAsync<T>(Func<T, string> identifierSelector, Func<Task<List<T>>> dataAccessor)
    {
        string objectIdentifier = null;
        var dataObjects = await dataAccessor();

        while (objectIdentifier is null)
        {
            objectIdentifier = GenerateRandomString();

            var existingObject = dataObjects.Where(s => identifierSelector(s) == objectIdentifier).FirstOrDefault();

            if (existingObject is not null)
            {
                // Object Identifier is already in use, generate a new one.
                objectIdentifier = null;
            }
        }

        return objectIdentifier;
    }

    public async Task<string> GenerateFriendCodeAsync()
    {
        return await GenerateCodeAsync(u => u.FriendCode, _userData.GetAllUsersAsync);
    }

    public async Task<string> GenerateServerInvationCodeAsync()
    {
        return await GenerateCodeAsync(s => s.InvitationCode, _serverData.GetAllServersAsync);
    }

    public async Task<string> GenerateRandomMessageIdentifier()
    {
        return await GenerateCodeAsync(m => m.ObjectIdentifier, _messageData.GetAllMessagesAsync);
    }

    public async Task<string> GenerateRandomConversationIdentifier()
    {
        return await GenerateCodeAsync(c => c.ObjectIdentifier, _conversationData.GetAllConversationAsync);
    }

    private static string GenerateRandomString()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
