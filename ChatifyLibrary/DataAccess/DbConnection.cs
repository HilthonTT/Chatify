using Microsoft.Extensions.Configuration;

namespace ChatifyLibrary.DataAccess;

public class DbConnection : IDbConnection
{
    private readonly IMongoDatabase _db;
    private readonly IConfiguration _config;
    private const string _connectionId = "MongoDB";

    public string DbName { get; private set; }
    public string ConversationCollectionName { get; private set; } = "conversations";
    public string MessageCollectionName { get; private set; } = "messages";
    public string UserCollectionName { get; private set; } = "users";
    public string CategoryCollectionName { get; private set; } = "categories";
    public string FriendRequestCollectionName { get; private set; } = "friend-requests";
    public string PrivateConversationCollectionName { get; private set; } = "private-conversations";
    public string PrivateMessageCollectionName { get; private set; } = "private-messages";
    public string BanCollectionName { get; private set; } = "bans";
    public string BanAppealCollectionName { get; private set; } = "ban-appeals";

    public MongoClient Client { get; private set; }
    public IMongoCollection<ConversationModel> ConversationCollection { get; private set; }
    public IMongoCollection<MessageModel> MessageCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; private set; }
    public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }
    public IMongoCollection<FriendRequestModel> FriendRequestCollection { get; private set; }
    public IMongoCollection<PrivateConversationModel> PrivateConversationCollection { get; private set; }
    public IMongoCollection<PrivateMessageModel> PrivateMessageCollection { get; private set; }
    public IMongoCollection<BanModel> BanCollection { get; private set; }
    public IMongoCollection<BanAppealModel> BanAppealCollection { get; private set; }

    public DbConnection(IConfiguration config)
    {
        _config = config;
        Client = new MongoClient(_config.GetConnectionString(_connectionId));
        DbName = _config["DatabaseName"];
        _db = Client.GetDatabase(DbName);

        ConversationCollection = _db.GetCollection<ConversationModel>(ConversationCollectionName);
        MessageCollection = _db.GetCollection<MessageModel>(MessageCollectionName);
        UserCollection = _db.GetCollection<UserModel>(UserCollectionName);
        CategoryCollection = _db.GetCollection<CategoryModel>(CategoryCollectionName);
        FriendRequestCollection = _db.GetCollection<FriendRequestModel>(FriendRequestCollectionName);
        PrivateConversationCollection = _db.GetCollection<PrivateConversationModel>(PrivateConversationCollectionName);
        PrivateMessageCollection = _db.GetCollection<PrivateMessageModel>(PrivateMessageCollectionName);
        BanCollection = _db.GetCollection<BanModel>(BanCollectionName);
        BanAppealCollection = _db.GetCollection<BanAppealModel>(BanAppealCollectionName);
    }
}
