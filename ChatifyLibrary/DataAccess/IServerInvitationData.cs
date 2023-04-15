namespace ChatifyLibrary.DataAccess
{
    public interface IServerInvitationData
    {
        Task CreateInvitation(ServerInvitationModel invitation);
        Task<List<ServerInvitationModel>> GetAllInvitationsAsync();
        Task<ServerInvitationModel> GetServerInvitationAsync(string id);
        Task<ServerInvitationModel> GetServerInvitationByServer(ServerModel server);
        Task<ServerInvitationModel> GetServerInvitationObjectIdAsync(string objectId);
    }
}