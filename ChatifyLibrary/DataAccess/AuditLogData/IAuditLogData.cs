namespace ChatifyLibrary.DataAccess.AuditLogData;

public interface IAuditLogData
{
    Task CreateAuditLog(AuditLogModel auditLog);
    Task<List<AuditLogModel>> GetAllServerAuditLogsAsync(ServerModel server);
    Task<AuditLogModel> GetAuditLogAsync(string id);
}