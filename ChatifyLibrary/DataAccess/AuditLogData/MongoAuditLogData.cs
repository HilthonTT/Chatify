namespace ChatifyLibrary.DataAccess.AuditLogData;

public class MongoAuditLogData : IAuditLogData
{
    private readonly IMongoCollection<AuditLogModel> _auditLogs;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _helper;

    public MongoAuditLogData(IDbConnection db,
                             IMemoryCache cache,
                             ICachingHelper helper)
    {
        _cache = cache;
        _helper = helper;
        _auditLogs = db.AuditLogCollection;
    }

    public async Task<List<AuditLogModel>> GetAllServerAuditLogsAsync(ServerModel server)
    {
        string cachingString = _helper.AuditLogCachingString(server.Id);

        var output = _cache.Get<List<AuditLogModel>>(cachingString);
        if (output is null)
        {
            output = await _auditLogs.Find(a => a.Server.Id == server.Id).ToListAsync();

            _cache.Set(cachingString, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<AuditLogModel> GetAuditLogAsync(string id)
    {
        var results = await _auditLogs.FindAsync(a => a.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateAuditLog(AuditLogModel auditLog)
    {
        return _auditLogs.InsertOneAsync(auditLog);
    }
}
