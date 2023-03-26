namespace ChatifyLibrary.DataAccess
{
    public interface IDbConnection
    {
        IMongoCollection<CategoryModel> CategoryCollection { get; }
        string CategoryCollectionName { get; }
        MongoClient Client { get; }
        IMongoCollection<ConversationModel> ConversationCollection { get; }
        string ConversationCollectionName { get; }
        string DbName { get; }
        IMongoCollection<FriendRequestModel> FriendRequestCollection { get; }
        string FriendRequestCollectionName { get; }
        IMongoCollection<MessageModel> MessageCollection { get; }
        string MessageCollectionName { get; }
        IMongoCollection<UserModel> UserCollection { get; }
        string UserCollectionName { get; }
    }
}