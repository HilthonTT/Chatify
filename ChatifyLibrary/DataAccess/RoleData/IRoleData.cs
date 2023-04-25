namespace ChatifyLibrary.DataAccess.RoleData;

public interface IRoleData
{
    Task CreateRole(RoleModel role);
    Task<RoleModel> CreateRoleAndReturn(RoleModel role);
    Task<List<RoleModel>> GetAllRolesServerAsync(ServerModel server);
    Task<RoleModel> GetRoleAsync(string id);
    Task<RoleModel> GetServerMemberRoleAsync(ServerModel server);
    Task<RoleModel> GetUserServerRoleAsync(UserModel user, ServerModel server);
    Task UpdateRole(RoleModel role);
}