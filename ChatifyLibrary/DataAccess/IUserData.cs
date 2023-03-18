namespace ChatifyLibrary.DataAccess;

public interface IUserData
{
    Task CreateUser(UserModel user);
    Task<UserModel> GetUserAsync(string id);
    Task<UserModel> GetUserFromAuthenticationAsync(string objectId);
    Task<List<UserModel>> GetAllUsersAsync();
    Task UpdateUser(UserModel user);
}