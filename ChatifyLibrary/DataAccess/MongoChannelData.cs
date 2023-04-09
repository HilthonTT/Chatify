namespace ChatifyLibrary.DataAccess;

public class MongoChannelData : IChannelData
{
    private readonly IMongoCollection<ChannelModel> _channels;
    private readonly IMemoryCache _cache;
    private const string CacheName = "ChannelData";

    public MongoChannelData(IDbConnection db,
                            IMemoryCache cache)
    {
        _cache = cache;
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

    public async Task<ChannelModel> GetChannelAsync(string id)
    {
        var results = await _channels.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateChannel(ChannelModel channel)
    {
        return _channels.InsertOneAsync(channel);
    }

    public Task UpdateChannel(ChannelModel channel)
    {
        var filter = Builders<ChannelModel>.Filter.Eq("Id", channel.Id);
        return _channels.ReplaceOneAsync(filter, channel, new ReplaceOptions { IsUpsert = true });
    }
}
