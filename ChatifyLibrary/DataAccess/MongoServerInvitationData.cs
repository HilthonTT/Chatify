using ChatifyLibrary.Helper;

namespace ChatifyLibrary.DataAccess;

public class MongoServerInvitationData : IServerInvitationData
{
    private readonly IMongoCollection<ServerInvitationModel> _invitations;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;
    private const string CacheName = "InvitationData";

    public MongoServerInvitationData(IDbConnection db,
                                     IMemoryCache cache,
                                     ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _invitations = db.ServerInvitationCollection;
    }

    public async Task<List<ServerInvitationModel>> GetAllInvitationsAsync()
    {
        var output = _cache.Get<List<ServerInvitationModel>>(CacheName);
        if (output is null)
        {
            var results = await _invitations.FindAsync(_ => true);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<ServerInvitationModel> GetServerInvitationAsync(string id)
    {
        string cachingString = _helper.ServerInvitationCachingString(id);

        var output = _cache.Get<ServerInvitationModel>(cachingString);
        if (output is null)
        {
            var results = await _invitations.FindAsync(i => i.Id == id);
            output = await results.FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<ServerInvitationModel> GetServerInvitationObjectIdAsync(string objectId)
    {
        string cachingString = _helper.ServerInvitationCachingString(objectId);

        var output = _cache.Get<ServerInvitationModel>(cachingString);
        if (output is null)
        {
            var results = await _invitations.FindAsync(i => i.ObjectIdentifier == objectId);
            output = await results.FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public async Task<ServerInvitationModel> GetServerInvitationByServer(ServerModel server)
    {
        string cachingString = _helper.ServerInvitationCachingString(server.Id);

        var output = _cache.Get<ServerInvitationModel>(cachingString);
        if (output is null)
        {
            var results = await _invitations.FindAsync(i => i.InvitationCode == server.InvitationCode);
            output = await results.FirstOrDefaultAsync();

            _cache.Set(cachingString, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateInvitation(ServerInvitationModel invitation)
    {
        return _invitations.InsertOneAsync(invitation);
    }
}
