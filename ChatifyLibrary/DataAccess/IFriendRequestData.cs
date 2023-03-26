namespace ChatifyLibrary.DataAccess
{
    public interface IFriendRequestData
    {
        Task CreateFriendRequest(FriendRequestModel request, UserModel user);
        Task<List<FriendRequestModel>> GetAllFriendRequestAsync();
        Task<FriendRequestModel> GetFriendRequestAsync(string id);
        Task<List<FriendRequestModel>> GetUserPendingFriendRequestsAsync(string userId);
        Task<List<FriendRequestModel>> GetUserSendedFriendRequestsAsync(string userId);
        Task UpdateFriendRequest(FriendRequestModel request, UserModel user);
    }
}