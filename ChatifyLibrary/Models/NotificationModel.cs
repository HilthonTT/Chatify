namespace ChatifyLibrary.Models;

public class NotificationModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Recipient { get; set; }
    public MessageModel Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSeen { get; set; }
}
