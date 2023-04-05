﻿namespace ChatifyLibrary.DataAccess;

public class MongoBanData : IBanData
{
    private readonly IMongoCollection<BanModel> _bans;
    private readonly IMemoryCache _cache;
    private const string CacheName = "BanData";

    public MongoBanData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _bans = db.BanCollection;
    }

    public async Task<List<BanModel>> GetAllBansAsync()
    {
        var output = _cache.Get<List<BanModel>>(CacheName);
        if (output is null)
        {
            var results = await _bans.FindAsync(_ => true);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<BanModel> GetBanAsync(string id)
    {
        var results = await _bans.FindAsync(b => b.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<BanModel> GetUserBanActive(string userId)
    {
        var output = _cache.Get<BanModel>(userId);
        if (output is null)
        {
            var filter = Builders<BanModel>.Filter.And(
                Builders<BanModel>.Filter.Eq(b => b.UserBanned.Id, userId),
                Builders<BanModel>.Filter.Where(b => b.BannedUntil < DateTime.UtcNow));

            output = await _bans.Find(filter).FirstOrDefaultAsync();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public Task CreateBan(BanModel ban)
    {
        return _bans.InsertOneAsync(ban);
    }

    public Task UpdateBan(BanModel ban)
    {
        var filter = Builders<BanModel>.Filter.Eq("Id", ban.Id);
        return _bans.ReplaceOneAsync(filter, ban, new ReplaceOptions { IsUpsert = true });
    }
}