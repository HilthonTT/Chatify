using ChatifyLibrary.Helper;
using System.Runtime.CompilerServices;

namespace ChatifyLibrary.DataAccess;

public class MongoPrivateConversationData : IPrivateConversationData
{
    private readonly IMongoCollection<PrivateConversationModel> _conversations;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "PrivateConversationData";

    public MongoPrivateConversationData(IDbConnection db,
                                        IMemoryCache cache,
                                        ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _conversations = db.PrivateConversationCollection;
    }

    public async Task<List<PrivateConversationModel>> GetAllConversationAsync()
    {
        var output = _cache.Get<List<PrivateConversationModel>>(CacheName);
        if (output is null)
        {
            var results = await _conversations.FindAsync(c => c.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<PrivateConversationModel> GetUsersConversationAsync(string firstUserId, string secondUserId)
    {
        string cachingString = _helper.PrivateConversationCachingString(firstUserId, secondUserId);

        var output = _cache.Get<PrivateConversationModel>(cachingString);
        if (output is null)
        {
            var filter = Builders<PrivateConversationModel>.Filter.Or(
                Builders<PrivateConversationModel>.Filter.And(
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.FirstParticipant.Id, firstUserId),
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.LastParticipant.Id, secondUserId)),
                Builders<PrivateConversationModel>.Filter.And(
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.FirstParticipant.Id, secondUserId),
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.LastParticipant.Id, firstUserId)));

            output = await _conversations.Find(filter).FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<PrivateConversationModel> GetConversationAsync(string id)
    {
        var results = await _conversations.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateConversation(PrivateConversationModel conversation)
    {
        return _conversations.InsertOneAsync(conversation);
    }

    public async Task UpdateConversation(PrivateConversationModel conversation)
    {
        var filter = Builders<PrivateConversationModel>.Filter.Eq("Id", conversation.Id);
        await _conversations.ReplaceOneAsync(filter, conversation, new ReplaceOptions { IsUpsert = true });
    }
}
