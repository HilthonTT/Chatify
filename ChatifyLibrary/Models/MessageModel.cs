namespace ChatifyLibrary.Models;

public class MessageModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Sender { get; set; }
    public BasicUserModel Receiver { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ConversationModel Conversation { get; set; }
    public bool Archived { get; set; }
}
