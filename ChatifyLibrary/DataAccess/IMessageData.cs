namespace ChatifyLibrary.DataAccess;

public interface IMessageData
{
    Task CreateMessage(MessageModel message);
    Task<List<MessageModel>> GetAllMessagesAsync();
    Task<MessageModel> GetMessageAsync(string id);
    Task UpdateMessageAsync(MessageModel message);
}