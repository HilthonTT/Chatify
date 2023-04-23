namespace ChatifyLibrary.DataAccess;

public interface IServerData
{
    Task CreateServer(ServerModel server);
    Task<ServerModel> CreateServerAndReturn(ServerModel server);
    Task<List<ServerModel>> GetAllServersAsync();
    Task<ServerModel> GetServerAsync(string id);
    Task<ServerModel> GetServerObjectIdAsync(string objectId);
    Task<List<ServerModel>> GetUserServersAsync(string userId);
    Task UpdateServer(ServerModel server);
}