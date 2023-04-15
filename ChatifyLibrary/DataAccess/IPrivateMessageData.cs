namespace ChatifyLibrary.DataAccess;

public interface IPrivateMessageData
{
    Task CreateMessage(PrivateMessageModel message);
    Task<List<PrivateMessageModel>> GetAllMessagesAsync();
    Task<List<PrivateMessageModel>> GetConversationMessagesAsync(PrivateConversationModel conversation);
    Task<PrivateMessageModel> GetMessageAsync(string id);
    Task<PrivateMessageModel> GetMessageObjectIdAsync(string objectId);
    Task UpdateMessageAsync(PrivateMessageModel message);
}