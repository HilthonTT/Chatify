using ChatifyLibrary.DataAccess;

namespace Chatify.Helpers;

public class CodeGenerator : ICodeGenerator
{
    private readonly IUserData _userData;
    private readonly IServerData _serverData;
    private readonly IMessageData _messageData;
    private readonly IConversationData _conversationData;
    private readonly IChannelData _channelData;
    private readonly IPrivateConversationData _privateConversationData;
    private readonly IFriendRequestData _requestData;

    public CodeGenerator(IUserData userData,
                         IServerData serverData,
                         IMessageData messageData,
                         IConversationData conversationData,
                         IChannelData channelData,
                         IPrivateConversationData privateConversationData,
                         IFriendRequestData requestData)
    {
        _userData = userData;
        _serverData = serverData;
        _messageData = messageData;
        _conversationData = conversationData;
        _channelData = channelData;
        _privateConversationData = privateConversationData;
        _requestData = requestData;
    }
    private static string GenerateRandomString()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-!@#$%^&*(){}|";
        return new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static async Task<string> GenerateCodeAsync<T>(Func<T, string> identifierSelector,
                                                           Func<Task<List<T>>> dataAccessor)
    {
        string objectIdentifier = null;
        var dataObjects = await dataAccessor();

        while (objectIdentifier is null)
        {
            objectIdentifier = GenerateRandomString();

            var existingObject = dataObjects.Where(
                s => identifierSelector(s) == objectIdentifier)
                .FirstOrDefault();

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

    public async Task<string> GenerateServerIdentifierAsync()
    {
        return await GenerateCodeAsync(s => s.ObjectIdentifier, _serverData.GetAllServersAsync);
    }

    public async Task<string> GenerateMessageIdentifierAsync()
    {
        return await GenerateCodeAsync(m => m.ObjectIdentifier, _messageData.GetAllMessagesAsync);
    }

    public async Task<string> GenerateConversationIdentifierAsync()
    {
        return await GenerateCodeAsync(c => c.ObjectIdentifier, _conversationData.GetAllConversationAsync);
    }

    public async Task<string> GenerateChannelIdentifierAsync()
    {
        return await GenerateCodeAsync(c=> c.ObjectIdentifier, _channelData.GetAllChannelsAsync);
    }

    public async Task<string> GeneratePrivateConversationIdentifierAsync()
    {
        return await GenerateCodeAsync(c => c.ObjectIdentifier, _privateConversationData.GetAllConversationAsync);
    }

    public async Task<string> GenerateFriendRequestIdentifierAsync()
    {
        return await GenerateCodeAsync(r => r.ObjectIdentifier, _requestData.GetAllFriendRequestAsync);
    }
}
