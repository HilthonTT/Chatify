﻿namespace ChatifyLibrary.DataAccess.MessageData;

public class MongoMessageData : IMessageData
{
    private readonly IMongoCollection<MessageModel> _messages;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "MessageData";

    public MongoMessageData(IDbConnection db,
                            IMemoryCache cache,
                            ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _messages = db.MessageCollection;
    }

    public async Task<List<MessageModel>> GetAllMessagesAsync()
    {
        var output = _cache.Get<List<MessageModel>>(CacheName);
        if (output is null)
        {
            var results = await _messages.FindAsync(m => m.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetConversationMessagesAsync(ConversationModel conversation)
    {
        string cachingString = _helper.MessageCachingString(conversation.Id);

        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Conversation.Id, conversation.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetConversationUnreadMessagesAsync(ConversationModel conversation,
                                                                             UserModel user)
    {
        string cachingString = _helper.MessageCachingString(conversation.Id + user.Id);

        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Conversation.Id, conversation.Id),
                Builders<MessageModel>.Filter.AnyNe(m => m.ReadBy.Select(u => u.Id), user.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetServerUnreadMessagesAsync(ServerModel server,
                                                                       UserModel user)
    {
        string cachingString = _helper.MessageCachingString(server.Id + user.Id);
        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Server.Id, server.Id),
                Builders<MessageModel>.Filter.AnyNe(m => m.ReadBy.Select(u => u.Id), user.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetPrivateConversationMessagesAsync(PrivateConversationModel conversation)
    {
        string cachingString = _helper.MessageCachingString(conversation.Id);

        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.PrivateConversation.Id, conversation.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetServerMessagesAsync(ServerModel server)
    {
        string cachingString = _helper.MessageCachingString(server.Id);

        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Server.Id, server.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<List<MessageModel>> GetChannelMessagesAsync(ChannelModel channel)
    {
        string cachingString = _helper.MessageCachingString(channel.Id);

        var output = _cache.Get<List<MessageModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<MessageModel>.Filter.And(
                Builders<MessageModel>.Filter.Eq(m => m.Channel.Id, channel.Id),
                Builders<MessageModel>.Filter.Eq(m => m.Archived, false));

            output = await _messages.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<MessageModel> GetMessageAsync(string id)
    {
        var results = await _messages.FindAsync(m => m.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<MessageModel> GetMessageObjectIdentifierAsync(MessageModel message)
    {
        var results = await _messages.FindAsync(
            m => m.ObjectIdentifier == message.ObjectIdentifier);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateMessage(MessageModel message)
    {
        return _messages.InsertOneAsync(message);
    }

    public async Task UpdateMessageAsync(MessageModel message)
    {
        var filter = Builders<MessageModel>.Filter.Eq("Id", message.Id);
        await _messages.ReplaceOneAsync(filter, message, new ReplaceOptions { IsUpsert = true });
    }
}
