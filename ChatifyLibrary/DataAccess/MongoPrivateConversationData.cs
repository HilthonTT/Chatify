namespace ChatifyLibrary.DataAccess;

public class MongoPrivateConversationData : IPrivateConversationData
{
    private readonly IMongoCollection<PrivateConversationModel> _conversations;
    private readonly IMemoryCache _cache;
    private const string CacheName = "PrivateConversationData";

    public MongoPrivateConversationData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _conversations = db.PrivateConversationCollection;
    }

    public async Task<List<PrivateConversationModel>> GetAllConversationAsync()
    {
        var output = _cache.Get<List<PrivateConversationModel>>(CacheName);
        if (output is null)
        {
            var results = await _conversations.FindAsync(c => c.Archived == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<PrivateConversationModel> GetUsersConversationAsync(string firstUserId, string secondUserId)
    {
        var output = _cache.Get<PrivateConversationModel>($"{firstUserId} - {secondUserId}");
        if (output is null)
        {
            var filter = Builders<PrivateConversationModel>.Filter.Or(
                Builders<PrivateConversationModel>.Filter.And(
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.FirstParticipant.Id, firstUserId),
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.LastParticipant.Id, secondUserId)),
                Builders<PrivateConversationModel>.Filter.And(
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.FirstParticipant.Id, secondUserId),
                    Builders<PrivateConversationModel>.Filter.Eq(c => c.LastParticipant.Id, firstUserId))
            );


            output = await _conversations.Find(filter).FirstOrDefaultAsync();

            _cache.Set($"{firstUserId} - {secondUserId}", output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<PrivateConversationModel> GetConversationAsync(string id)
    {
        var results = await _conversations.FindAsync(c => c.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateConversation(PrivateConversationModel conversation)
    {
        return _conversations.InsertOneAsync(conversation);
    }

    public async Task UpdateConversation(PrivateConversationModel conversation)
    {
        await _conversations.ReplaceOneAsync(c => c.Id == conversation.Id, conversation);
        _cache.Remove(CacheName);
    }
}
