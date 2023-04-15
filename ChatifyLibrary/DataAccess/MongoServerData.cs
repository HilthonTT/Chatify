using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess;

public class MongoServerData : IServerData
{
    private readonly IMongoCollection<ServerModel> _servers;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "ServerData";

    public MongoServerData(IDbConnection db,
                           IMemoryCache cache,
                           ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _servers = db.ServerCollection;
    }

    public async Task<List<ServerModel>> GetAllServersAsync()
    {
        var output = _cache.Get<List<ServerModel>>(CacheName);
        if (output is null)
        {
            var results = await _servers.FindAsync(s => s.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<ServerModel>> GetUserServersAsync(string userId)
    {
        string cachingString = _helper.ServerCachingString(userId);

        var output = _cache.Get<List<ServerModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<ServerModel>.Filter.And(
                Builders<ServerModel>.Filter.AnyEq(s => s.Members.Select(m => m.Id), userId),
                Builders<ServerModel>.Filter.Eq(s => s.Archived, false));

            output = await _servers.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<ServerModel> GetServerAsync(string id)
    {
        var results = await _servers.FindAsync(s => s.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<ServerModel> GetServerObjectIdAsync(string objectId)
    {
        var results = await _servers.FindAsync(s => s.ObjectIdentifier == objectId);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<ServerModel> GetServerByInvitationCodeAsync(string invitationCode)
    {
        var results = await _servers.FindAsync(s => s.InvitationCode == invitationCode);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateServer(ServerModel server)
    {
        return _servers.InsertOneAsync(server);
    }

    public Task UpdateServer(ServerModel server)
    {
        var filter = Builders<ServerModel>.Filter.Eq("Id", server.Id);
        return _servers.ReplaceOneAsync(filter, server, new ReplaceOptions { IsUpsert = true });
    }
}
