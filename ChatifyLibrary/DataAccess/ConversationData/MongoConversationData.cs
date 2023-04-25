using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess.ConversationData;

public class MongoConversationData : IConversationData
{
    private readonly IMongoCollection<ConversationModel> _conversations;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "ConversationData";

    public MongoConversationData(IDbConnection db,
                                 IMemoryCache cache,
                                 ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _conversations = db.ConversationCollection;
    }

    public async Task<List<ConversationModel>> GetUserConversationsAsync(string userId)
    {
        string cachingString = _helper.ConversationCachingString(userId);

        var output = _cache.Get<List<ConversationModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<ConversationModel>.Filter.And(
                Builders<ConversationModel>.Filter.AnyEq(c => c.Participants.Select(p => p.Id), userId),
                Builders<ConversationModel>.Filter.Eq(c => c.Archived, false));

            output = await _conversations.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
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

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<ConversationModel> GetConversationAsync(string id)
    {
        var results = await _conversations.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<ConversationModel> GetConversationByObjectIdentifier(string objectId)
    {
        var results = await _conversations.FindAsync(c => c.ObjectIdentifier == objectId);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateConversation(ConversationModel conversation)
    {
        return _conversations.InsertOneAsync(conversation);
    }

    public async Task UpdateConversation(ConversationModel conversation)
    {
        var filter = Builders<ConversationModel>.Filter.Eq("Id", conversation.Id);
        await _conversations.ReplaceOneAsync(filter, conversation, new ReplaceOptions { IsUpsert = true });
    }
}
