namespace ChatifyLibrary.DataAccess;

public interface INotificationData
{
    Task CreateNotification(NotificationModel notification);
    Task<List<NotificationModel>> GetAllNotificationsAsync();
    Task<NotificationModel> GetNotificationAsync(string id);
    Task<List<NotificationModel>> GetUserNotificationAsync(string userId);
}