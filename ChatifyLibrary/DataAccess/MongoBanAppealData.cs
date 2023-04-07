namespace ChatifyLibrary.DataAccess;

public class MongoBanAppealData : IBanAppealData
{
    private readonly IMongoCollection<BanAppealModel> _banAppeals;
    private readonly IMemoryCache _cache;
    private const string CacheName = "BanAppealsData";

    public MongoBanAppealData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
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
        var output = _cache.Get<BanAppealModel>(ban.Id);
        if (output is null)
        {
            output = await _banAppeals.Find(b => b.Ban.Id == ban.Id).FirstOrDefaultAsync();

            _cache.Set(ban.Id, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public Task CreateBanAppeal(BanAppealModel appeal)
    {
        return _banAppeals.InsertOneAsync(appeal);
    }

    public Task UpdateAppeal(BanAppealModel appeal)
    {
        var filter = Builders<BanAppealModel>.Filter.Eq("Id", appeal.Id);
        return _banAppeals.ReplaceOneAsync(filter, appeal, new ReplaceOptions { IsUpsert = true });
    }
}
