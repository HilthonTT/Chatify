namespace ChatifyLibrary.DataAccess.ChannelData;

public class MongoChannelData : IChannelData
{
    private readonly IMongoCollection<ChannelModel> _channels;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "ChannelData";

    public MongoChannelData(IDbConnection db,
                            IMemoryCache cache,
                            ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _channels = db.ChannelCollection;
    }

    public async Task<List<ChannelModel>> GetAllChannelsAsync()
    {
        var output = _cache.Get<List<ChannelModel>>(CacheName);
        if (output is null)
        {
            var results = await _channels.FindAsync(s => s.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<List<ChannelModel>> GetAllChannelsServerAsync(ServerModel server)
    {
        string cachingString = _helper.ChannelCachingString(server.Id);

        var output = _cache.Get<List<ChannelModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<ChannelModel>.Filter.And(
                Builders<ChannelModel>.Filter.Eq(c => c.Server.Id, server.Id),
                Builders<ChannelModel>.Filter.Eq(c => c.Archived, false));

            output = await _channels.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<List<ChannelModel>> GetAllChannelsCategoryAsync(ChannelCategoryModel category)
    {
        string cachingString = _helper.ChannelCachingString(category.Id);

        var output = _cache.Get<List<ChannelModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<ChannelModel>.Filter.And(
                Builders<ChannelModel>.Filter.Eq(c => c.Category.Id, category.Id),
                Builders<ChannelModel>.Filter.Eq(c => c.Archived, false));

            output = await _channels.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string id)
    {
        var results = await _channels.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<ChannelModel> GetChannelObjectIdAsync(string objectId)
    {
        var results = await _channels.FindAsync(c => c.ObjectIdentifier == objectId);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateChannel(ChannelModel channel)
    {
        string cachingString = _helper.ChannelCachingString(channel.Server.Id);

        _cache.Remove(cachingString);
        return _channels.InsertOneAsync(channel);
    }

    public async Task<ChannelModel> CreateChannelAndReturn(ChannelModel channel)
    {
        string cachingString = _helper.ChannelCachingString(channel.Server.Id);

        _cache.Remove(cachingString);
        await _channels.InsertOneAsync(channel);
        return channel;
    }

    public Task UpdateChannel(ChannelModel channel)
    {
        var filter = Builders<ChannelModel>.Filter.Eq("Id", channel.Id);
        return _channels.ReplaceOneAsync(filter, channel, new ReplaceOptions { IsUpsert = true });
    }
}
