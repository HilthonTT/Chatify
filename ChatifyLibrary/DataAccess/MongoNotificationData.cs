using Microsoft.Extensions.Caching.Memory;

namespace ChatifyLibrary.DataAccess;

public class MongoNotificationData : INotificationData
{
    private readonly IMongoCollection<NotificationModel> _notifications;
    private readonly IMemoryCache _cache;
    private const string CacheName = "NotificationData";

    public MongoNotificationData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _notifications = db.NotificationCollection;
    }

    public async Task<List<NotificationModel>> GetUserNotificationAsync(string userId)
    {
        var output = _cache.Get<List<NotificationModel>>(userId);
        if (output is null)
        {
            var results = await _notifications.FindAsync(n => n.Recipient.Id == userId);
            output = await results.ToListAsync();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<NotificationModel>> GetAllNotificationsAsync()
    {
        var output = _cache.Get<List<NotificationModel>>(CacheName);
        if (output is null)
        {
            var results = await _notifications.FindAsync(n => n.IsSeen == false);
            output = await results.ToListAsync();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<NotificationModel> GetNotificationAsync(string id)
    {
        var results = await _notifications.FindAsync(n => n.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public Task CreateNotification(NotificationModel notification)
    {
        return _notifications.InsertOneAsync(notification);
    }
}
