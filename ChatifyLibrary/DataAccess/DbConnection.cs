using Microsoft.Extensions.Configuration;

namespace ChatifyLibrary.DataAccess;

public class DbConnection : IDbConnection
{
    private readonly IMongoDatabase _db;
    private readonly IConfiguration _config;
    private const string _connectionId = "MongoDB";

    public string DbName { get; set; }
    public string ConversationCollectionName { get; set; } = "conversations";
    public string MessageCollectionName { get; set; } = "messages";
    public string NotificationCollectionName { get; set; } = "notifications";
    public string UserCollectionName { get; set; } = "users";

    public MongoClient Client { get; set; }
    public IMongoCollection<ConversationModel> ConversationCollection { get; private set; }
    public IMongoCollection<MessageModel> MessageCollection { get; private set; }
    public IMongoCollection<NotificationModel> NotificationCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; private set; }

    public DbConnection(IConfiguration config)
    {
        _config = config;
        Client = new MongoClient(_config.GetConnectionString(_connectionId));
        DbName = _config["DatabaseName"];
        _db = Client.GetDatabase(DbName);

        ConversationCollection = _db.GetCollection<ConversationModel>(ConversationCollectionName);
        MessageCollection = _db.GetCollection<MessageModel>(MessageCollectionName);
        NotificationCollection = _db.GetCollection<NotificationModel>(NotificationCollectionName);
        UserCollection = _db.GetCollection<UserModel>(UserCollectionName);
    }
}
