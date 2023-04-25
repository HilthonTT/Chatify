namespace ChatifyLibrary.DataAccess.FriendRequestData;

public interface IFriendRequestData
{
    Task CreateFriendRequest(FriendRequestModel request);
    Task DeleteFriendRequestAsync(FriendRequestModel request);
    Task<List<FriendRequestModel>> GetAllFriendRequestAsync();
    Task<FriendRequestModel> GetFriendRequestAsync(string id);
    Task<FriendRequestModel> GetFriendRequestObjectIdAsync(string objectId);
    Task<FriendRequestModel> GetAlreadySendedFriendRequestAsync(UserModel sender, UserModel receiver);
    Task<List<FriendRequestModel>> GetUserPendingFriendRequestsAsync(string userId);
    Task<List<FriendRequestModel>> GetUserSendedFriendRequestsAsync(string userId);
    Task UpdateFriendRequest(FriendRequestModel request);
    Task<FriendRequestModel> GetSenderAndReceiverFriendRequestAsync(UserModel sender, UserModel receiver);
}