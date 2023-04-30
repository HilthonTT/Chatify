namespace ChatifyLibrary.DataAccess.UserData;

public class MongoUserData : IUserData
{
    private readonly IMongoCollection<UserModel> _users;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "UserData";

    public MongoUserData(IDbConnection db,
                         IMemoryCache cache,
                         ICachingHelper helper)
    {
        _users = db.UserCollection;
        _cache = cache;
        _helper = helper;
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        var results = await _users.FindAsync(_ => true);
        return await results.ToListAsync();
    }

    public async Task<List<UserModel>> GetAllUsersCachedAsync()
    {
        var output = _cache.Get<List<UserModel>>(CacheName);
        if (output is null)
        {
            var results = await _users.FindAsync(_ => true);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<UserModel>> GetAllUsersServerAsync(ServerModel server)
    {
        string cachingString = _helper.UserCachingString(server.Id);

        var output = _cache.Get<List<UserModel>>(cachingString);
        if (output is null)
        {
            var memberIds = server.Members.Select(m => m.Id).ToList();
            var filter = Builders<UserModel>.Filter.In(u => u.Id, memberIds);
            output = await _users.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<List<BasicUserModel>> GetUserFriendsAsync(UserModel user)
    {
        string cachingString = _helper.UserCachingString(user.Id);

        var output = _cache.Get<List<BasicUserModel>>(cachingString);
        if (output is null)
        {
            if (user.Friends == null || user.Friends.Count == 0)
            {
                return new List<BasicUserModel>();
            }

            var friendIds = user.Friends.Select(f => f.Id).ToList();
            var filter = Builders<UserModel>.Filter.In(u => u.Id, friendIds);
            output = await _users.Find(filter).Project(u => new BasicUserModel{ 
                Id = u.Id, 
                FirstName = u.FirstName, 
                LastName = u.LastName, 
                DisplayName = u.DisplayName,
                FileName = u.FileName,
                DateCreated = u.DateCreated }).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(30));
        }

        return output;
    }

    public async Task<UserModel> GetUserAsync(string id)
    {
        var results = await _users.FindAsync(u => u.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<UserModel> GetUserFromAuthenticationAsync(string objectId)
    {
        var results = await _users.FindAsync(u => u.ObjectIdentifier == objectId);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<UserModel> GetUserFriendCodeAsync(string code)
    {
        var results = await _users.FindAsync(u => u.FriendCode == code);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateUser(UserModel user)
    {
        return _users.InsertOneAsync(user);
    }

    public Task UpdateUser(UserModel user)
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);
        return _users.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
    }
}
