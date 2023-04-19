using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess;

public class MongoRoleData : IRoleData
{
    private readonly IMongoCollection<RoleModel> _roles;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;

    public MongoRoleData(IDbConnection db,
                         IMemoryCache cache,
                         ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _roles = db.RoleCollection;
    }

    public async Task<List<RoleModel>> GetAllRolesServerAsync(ServerModel server)
    {
        string cachingString = _helper.RoleCachingString(server.Id);

        var output = _cache.Get<List<RoleModel>>(cachingString);
        if (output is null)
        {
            var filter = Builders<RoleModel>.Filter.And(
                Builders<RoleModel>.Filter.Eq(r => r.Server.Id, server.Id),
                Builders<RoleModel>.Filter.Eq(r => r.Archived, false));

            output = await _roles.Find(filter).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(30));
        }

        return output;
    }

    public async Task<RoleModel> GetUserServerRoleAsync(UserModel user, ServerModel server)
    {
        string cachingString = _helper.RoleCachingString(user.Id + "-" + server.Id);

        var output = _cache.Get<RoleModel>(cachingString);
        if (output is null)
        {
            var filter = Builders<RoleModel>.Filter.And(
                    Builders<RoleModel>.Filter.Eq(r => r.Server.Id, server.Id),
                    Builders<RoleModel>.Filter.ElemMatch(
                        r => r.Users,
                        u => u.Id == user.Id),
                    Builders<RoleModel>.Filter.Eq(r => r.Archived, false));

            var results = await _roles.FindAsync(filter);

            output = await results.FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromMinutes(30));
        }

        return output;
    }

    public async Task<RoleModel> GetRoleAsync(string id)
    {
        var results = await _roles.FindAsync(r => r.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateRole(RoleModel role)
    {
        return _roles.InsertOneAsync(role);
    }

    public Task UpdateRole(RoleModel role)
    {
        var filter = Builders<RoleModel>.Filter.Eq("Id", role.Id);
        return _roles.ReplaceOneAsync(filter, role, new ReplaceOptions { IsUpsert = true });
    }
}
