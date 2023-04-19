namespace ChatifyLibrary.DataAccess;

public interface IConversationData
{
    Task CreateConversation(ConversationModel conversation);
    Task<List<ConversationModel>> GetAllConversationAsync();
    Task<ConversationModel> GetConversationAsync(string id);
    Task<ConversationModel> GetConversationByObjectIdentifier(string objectId);
    Task<List<ConversationModel>> GetUserConversationsAsync(string userId);
    Task UpdateConversation(ConversationModel conversation);
}