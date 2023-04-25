using ChatifyLibrary.DataAccess.ChannelData;
using ChatifyLibrary.DataAccess.ConversationData;
using ChatifyLibrary.DataAccess.FriendRequestData;
using ChatifyLibrary.DataAccess.MessageData;
using ChatifyLibrary.DataAccess.PrivateConversationData;
using ChatifyLibrary.DataAccess.ChannelCategoryData;
using ChatifyLibrary.DataAccess.ServerData;
using ChatifyLibrary.DataAccess.UserData;

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
    private readonly IChannelCategoryData _channelCategoryData;

    public CodeGenerator(IUserData userData,
                         IServerData serverData,
                         IMessageData messageData,
                         IConversationData conversationData,
                         IChannelData channelData,
                         IPrivateConversationData privateConversationData,
                         IFriendRequestData requestData,
                         IChannelCategoryData channelCategoryData)
    {
        _userData = userData;
        _serverData = serverData;
        _messageData = messageData;
        _conversationData = conversationData;
        _channelData = channelData;
        _privateConversationData = privateConversationData;
        _requestData = requestData;
        _channelCategoryData = channelCategoryData;
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

    public async Task<string> GenerateChannelCategoryIdentifierAsync()
    {
        return await GenerateCodeAsync(c => c.ObjectIdentifier, _channelCategoryData.GetAllCategoriesAsync);
    }
}
