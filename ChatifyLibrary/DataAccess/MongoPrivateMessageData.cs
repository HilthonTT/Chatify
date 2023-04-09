using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess;

public class MongoPrivateMessageData : IPrivateMessageData
{
    private readonly IMongoCollection<PrivateMessageModel> _messages;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "PrivateMessageData";

    public MongoPrivateMessageData(IDbConnection db,
                                   IMemoryCache cache,
                                   ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _messages = db.PrivateMessageCollection;
    }

    public async Task<List<PrivateMessageModel>> GetAllMessagesAsync()
    {
        var output = _cache.Get<List<PrivateMessageModel>>(CacheName);
        if (output is null)
        {
            var results = await _messages.FindAsync(m => m.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<List<PrivateMessageModel>> GetConversationMessagesAsync(PrivateConversationModel conversation)
    {
        string cachingString = _helper.PrivateMessageCachingString(conversation.Id);

        var output = _cache.Get<List<PrivateMessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<PrivateMessageModel>.Filter.And(
                Builders<PrivateMessageModel>.Filter.Eq(m => m.Conversation.Id, conversation.Id),
                Builders<PrivateMessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<PrivateMessageModel> GetMessageAsync(string id)
    {
        var results = await _messages.FindAsync(m => m.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateMessage(PrivateMessageModel message)
    {
        return _messages.InsertOneAsync(message);
    }

    public async Task UpdateMessageAsync(PrivateMessageModel message)
    {
        var filter = Builders<PrivateMessageModel>.Filter.Eq("Id", message.Id);
        await _messages.ReplaceOneAsync(filter, message, new ReplaceOptions { IsUpsert = true });
    }
}
