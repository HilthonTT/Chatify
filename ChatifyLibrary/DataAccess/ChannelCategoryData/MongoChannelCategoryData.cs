namespace ChatifyLibrary.DataAccess.ServerCategoryData;

public class MongoChannelCategoryData : IChannelCategoryData
{
    private readonly IMongoCollection<ChannelCategoryModel> _channelCategories;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "ChannelCategoryData";

    public MongoChannelCategoryData(IDbConnection db,
                                    IMemoryCache cache,
                                    ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _channelCategories = db.ChannelCategoryCollection;
    }

    public async Task<List<ChannelCategoryModel>> GetAllCategoriesAsync()
    {
        var output = _cache.Get<List<ChannelCategoryModel>>(CacheName);
        if (output is null)
        {
            var results = await _channelCategories.FindAsync(s => s.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<ChannelCategoryModel>> GetServerCategoriesAsync(ServerModel server)
    {
        string cachingString = _helper.ChannelCategoryCachingString(server.Id);

        var output = _cache.Get<List<ChannelCategoryModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<ChannelCategoryModel>.Filter.And(
                Builders<ChannelCategoryModel>.Filter.Eq(c => c.Server.Id, server.Id),
                Builders<ChannelCategoryModel>.Filter.Eq(c => c.Archived, false));

            var results = await _channelCategories.FindAsync(filter);
            output = await results.ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(30));
        }

        return output;
    }

    public Task CreateCategory(ChannelCategoryModel category)
    {
        return _channelCategories.InsertOneAsync(category);
    }

    public async Task<ChannelCategoryModel> CreateCategoryAndReturn(ChannelCategoryModel category)
    {
        await _channelCategories.InsertOneAsync(category);
        return category;
    }

    public Task UpdateCategory(ChannelCategoryModel category)
    {
        var filter = Builders<ChannelCategoryModel>.Filter.Eq("Id", category.Id);
        return _channelCategories.ReplaceOneAsync(filter, category, new ReplaceOptions { IsUpsert = true });
    }
}
