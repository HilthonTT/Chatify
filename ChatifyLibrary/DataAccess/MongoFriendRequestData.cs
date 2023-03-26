namespace ChatifyLibrary.DataAccess;

public class MongoFriendRequestData : IFriendRequestData
{
    private readonly IMongoCollection<FriendRequestModel> _friendsRequest;
    private readonly IMemoryCache _cache;
    private const string CacheName = "FriendRequestData";
    private const string PendingCacheName = "Pending-Request";

    public MongoFriendRequestData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _friendsRequest = db.FriendRequestCollection;
    }

    public async Task<List<FriendRequestModel>> GetUserPendingFriendRequestsAsync(string userId)
    {
        var output = _cache.Get<List<FriendRequestModel>>($"{PendingCacheName}-{userId}");
        if (output is null)
        {
            var filter = Builders<FriendRequestModel>.Filter.And(
                Builders<FriendRequestModel>.Filter.Eq(f => f.Receiver.Id, userId),
                Builders<FriendRequestModel>.Filter.Eq(f => f.IsAccepted, false));

            output = await _friendsRequest.Find(filter).ToListAsync();

            _cache.Set($"{PendingCacheName}-{userId}", output, TimeSpan.FromMinutes(10));
        }

        return output;
    }

    public async Task<List<FriendRequestModel>> GetAllFriendRequestAsync()
    {
        var output = _cache.Get<List<FriendRequestModel>>(CacheName);
        if (output is null)
        {
            var results = await _friendsRequest.FindAsync(_ => true);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<FriendRequestModel> GetFriendRequestAsync(string id)
    {
        var results = await _friendsRequest.FindAsync(f => f.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateFriendRequest(FriendRequestModel request)
    {
        return _friendsRequest.InsertOneAsync(request);
    }

    public async Task UpdateFriendRequest(FriendRequestModel request)
    {
        await _friendsRequest.ReplaceOneAsync(f => f.Id == request.Id, request);
        _cache.Remove(CacheName);
    }
}
