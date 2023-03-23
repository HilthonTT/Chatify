using Microsoft.Extensions.Caching.Memory;

namespace ChatifyLibrary.DataAccess;

public class MongoMessageData : IMessageData
{
    private readonly IMongoCollection<MessageModel> _messages;
    private readonly IMemoryCache _cache;
    private const string CacheName = "MessageData";

    public MongoMessageData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _messages = db.MessageCollection;
    }

    public async Task<List<MessageModel>> GetAllMessagesAsync()
    {
        var output = _cache.Get<List<MessageModel>>(CacheName);
        if (output is null)
        {
            var results = await _messages.FindAsync(m => m.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetConversationMessagesAsync(ConversationModel conversation)
    {
        var output = _cache.Get<List<MessageModel>>(conversation.Id);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Conversation.Id, conversation.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(conversation.Id, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<MessageModel> GetMessageAsync(string id)
    {
        var results = await _messages.FindAsync(m => m.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateMessage(MessageModel message)
    {
        return _messages.InsertOneAsync(message);
    }

    public async Task UpdateMessageAsync(MessageModel message)
    {
        await _messages.ReplaceOneAsync(m => m.Id == message.Id, message);
        _cache.Remove(CacheName);
    }
}
