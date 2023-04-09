using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess;

public class MongoBanAppealData : IBanAppealData
{
    private readonly IMongoCollection<BanAppealModel> _banAppeals;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "BanAppealsData";

    public MongoBanAppealData(IDbConnection db,
                              IMemoryCache cache,
                              ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _banAppeals = db.BanAppealCollection;
    }

    public async Task<List<BanAppealModel>> GetAllBanAppealsAsync()
    {
        var output = _cache.Get<List<BanAppealModel>>(CacheName);
        if (output is null)
        {
            var results = await _banAppeals.FindAsync(_ => true);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<BanAppealModel> GetBanAppealAsync(string id)
    {
        var results = await _banAppeals.FindAsync(b => b.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<BanAppealModel> GetBanAppealFromBan(BanModel ban)
    {
        string cachingString = _helper.BanAppealCachingString(ban.Id);

        var output = _cache.Get<BanAppealModel>(cachingString);
        if (output is null)
        {
            output = await _banAppeals.Find(b => b.Ban.Id == ban.Id).FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public Task CreateBanAppeal(BanAppealModel appeal)
    {
        return _banAppeals.InsertOneAsync(appeal);
    }

    public async Task UpdateAppeal(BanAppealModel appeal)
    {
        var filter = Builders<BanAppealModel>.Filter.Eq("Id", appeal.Id);
        await _banAppeals.ReplaceOneAsync(filter, appeal, new ReplaceOptions { IsUpsert = true });
        _cache.Remove(CacheName);
    }

    public Task DeleteAppeal(BanAppealModel appeal)
    {
        var filter = Builders<BanAppealModel>.Filter.Eq("Id", appeal.Id);
        return _banAppeals.DeleteOneAsync(filter);
    }
}
