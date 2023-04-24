namespace ChatifyLibrary.DataAccess;

public interface IMessageData
{
    Task CreateMessage(MessageModel message);
    Task<List<MessageModel>> GetAllMessagesAsync();
    Task<List<MessageModel>> GetChannelMessagesAsync(ChannelModel channel);
    Task<List<MessageModel>> GetConversationMessagesAsync(ConversationModel conversation);
    Task<MessageModel> GetMessageAsync(string id);
    Task<MessageModel> GetMessageObjectIdentifierAsync(MessageModel message);
    Task<List<MessageModel>> GetPrivateConversationMessagesAsync(PrivateConversationModel conversation);
    Task<List<MessageModel>> GetServerMessagesAsync(ServerModel server);
    Task UpdateMessageAsync(MessageModel message);
}