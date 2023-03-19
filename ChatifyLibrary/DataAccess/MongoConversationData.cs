using Microsoft.Extensions.Caching.Memory;

namespace ChatifyLibrary.DataAccess;

public class MongoConversationData : IConversationData
{
    private readonly IMongoCollection<ConversationModel> _conversations;
    private readonly IMemoryCache _cache;
    private const string CacheName = "ConversationData";

    public MongoConversationData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _conversations = db.ConversationCollection;
    }

    public async Task<List<ConversationModel>> GetUserConversationsAsync(string userId)
    {
        var output = _cache.Get<List<ConversationModel>>(userId);
        if (output is null)
        {
            var filter = Builders<ConversationModel>.Filter.And(
                Builders<ConversationModel>.Filter.AnyEq(c => c.Participants.Select(p => p.Id), userId),
                Builders<ConversationModel>.Filter.Eq(c => c.Archived, false));

            output = await _conversations.Find(filter).ToListAsync();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<ConversationModel>> GetAllConversationAsync()
    {
        var output = _cache.Get<List<ConversationModel>>(CacheName);
        if (output is null)
        {
            var results = await _conversations.FindAsync(c => c.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<ConversationModel> GetConversationAsync(string id)
    {
        var results = await _conversations.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateConversation(ConversationModel conversation)
    {
        return _conversations.InsertOneAsync(conversation);
    }

    public async Task UpdateConversation(ConversationModel conversation, UserModel user)
    {
        await _conversations.ReplaceOneAsync(c => c.Id == conversation.Id, conversation);
        _cache.Remove(CacheName);
        _cache.Remove(user.Id);
    }
}
