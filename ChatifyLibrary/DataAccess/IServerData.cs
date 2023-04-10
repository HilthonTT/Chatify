﻿namespace ChatifyLibrary.DataAccess;

public interface IServerData
{
    Task CreateServer(ServerModel server);
    Task<List<ServerModel>> GetAllServersAsync();
    Task<ServerModel> GetServerAsync(string id);
    Task<ServerModel> GetServerByInvitationCodeAsync(string invitationCode);
    Task<List<ServerModel>> GetUserServersAsync(string userId);
    Task UpdateServerModel(ServerModel server);
}