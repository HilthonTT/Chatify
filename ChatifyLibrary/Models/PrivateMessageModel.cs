namespace ChatifyLibrary.Models;

public class PrivateMessageModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Sender { get; set; }
    public string Text { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string FileExtension { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime LastEdited { get; set; } = DateTime.UtcNow;
    public PrivateConversationModel Conversation { get; set; }
    public bool Archived { get; set; }
}
