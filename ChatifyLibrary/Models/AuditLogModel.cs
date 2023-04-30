namespace ChatifyLibrary.Models;

public class AuditLogModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public BasicUserModel User { get; set; }
    public BasicServerModel Server { get; set; }
    public BasicConversationModel Conversation { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public DateTime DateModified { get; set; } = DateTime.UtcNow;
}
