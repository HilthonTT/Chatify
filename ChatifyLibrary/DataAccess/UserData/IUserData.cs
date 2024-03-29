﻿namespace ChatifyLibrary.DataAccess.UserData;

public interface IUserData
{
    Task CreateUser(UserModel user);
    Task<UserModel> GetUserAsync(string id);
    Task<UserModel> GetUserFromAuthenticationAsync(string objectId);
    Task<List<UserModel>> GetAllUsersAsync();
    Task UpdateUser(UserModel user);
    Task<UserModel> GetUserFriendCodeAsync(string code);
    Task<List<UserModel>> GetAllUsersServerAsync(ServerModel server);
    Task<List<UserModel>> GetAllUsersCachedAsync();
    Task<List<BasicUserModel>> GetUserFriendsAsync(UserModel user);
}