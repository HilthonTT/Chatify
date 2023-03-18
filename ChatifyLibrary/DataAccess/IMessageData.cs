namespace ChatifyLibrary.DataAccess;

public interface IMessageData
{
    Task CreateMessage(MessageModel message);
    Task<List<MessageModel>> GetAllMessagesAsync();
    Task<MessageModel> GetMessageAsync(string id);
    Task<List<MessageModel>> GetReceivedMessagesAsync(string userId);
    Task<List<MessageModel>> GetSendedMessagesAsync(string userId);
    Task UpdateMessageAsync(MessageModel message);
}