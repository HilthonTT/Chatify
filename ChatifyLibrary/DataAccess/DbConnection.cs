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
    public string NotificationCollectionName { private get; set; } = "notifications";
    public string UserCollectionName { get; private set; } = "users";
    public string CategoryCollectionName { get; private set; } = "categories";

    public MongoClient Client { get; private set; }
    public IMongoCollection<ConversationModel> ConversationCollection { get; private set; }
    public IMongoCollection<MessageModel> MessageCollection { get; private set; }
    public IMongoCollection<NotificationModel> NotificationCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; private set; }
    public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }

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
        CategoryCollection = _db.GetCollection<CategoryModel>(CategoryCollectionName);
    }
}
