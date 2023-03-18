namespace ChatifyLibrary.DataAccess;

public interface IDbConnection
{
    MongoClient Client { get; set; }
    IMongoCollection<ConversationModel> ConversationCollection { get; }
    string ConversationCollectionName { get; set; }
    string DbName { get; set; }
    IMongoCollection<MessageModel> MessageCollection { get; }
    string MessageCollectionName { get; set; }
    IMongoCollection<NotificationModel> NotificationCollection { get; }
    string NotificationCollectionName { get; set; }
    IMongoCollection<UserModel> UserCollection { get; }
    string UserCollectionName { get; set; }
}