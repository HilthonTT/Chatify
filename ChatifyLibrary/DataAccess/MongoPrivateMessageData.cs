namespace ChatifyLibrary.DataAccess;

public class MongoPrivateMessageData : IPrivateMessageData
{
    private readonly IMongoCollection<PrivateMessageModel> _messages;
    private readonly IMemoryCache _cache;
    private const string CacheName = "PrivateMessageData";

    public MongoPrivateMessageData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _messages = db.PrivateMessageCollection;
    }

    public async Task<List<PrivateMessageModel>> GetAllMessagesAsync()
    {
        var output = _cache.Get<List<PrivateMessageModel>>(CacheName);
        if (output is null)
        {
            var results = await _messages.FindAsync(m => m.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<PrivateMessageModel>> GetConversationMessagesAsync(PrivateConversationModel conversation)
    {
        var output = _cache.Get<List<PrivateMessageModel>>(conversation.Id);
        if (output is null)
        {
            var filter = Builders<PrivateMessageModel>.Filter.And(
                Builders<PrivateMessageModel>.Filter.Eq(m => m.Conversation.Id, conversation.Id),
                Builders<PrivateMessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(conversation.Id, output, TimeSpan.FromMinutes(1));
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
        await _messages.ReplaceOneAsync(m => m.Id == message.Id, message);
        _cache.Remove(CacheName);
    }
}
