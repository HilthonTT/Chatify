namespace ChatifyLibrary.DataAccess;

public interface IPrivateConversationData
{
    Task CreateConversation(PrivateConversationModel conversation);
    Task<List<PrivateConversationModel>> GetAllConversationAsync();
    Task<PrivateConversationModel> GetConversationAsync(string id);
    Task<PrivateConversationModel> GetUsersConversationAsync(string firstUserId, string secondUserId);
    Task UpdateConversation(PrivateConversationModel conversation);
}