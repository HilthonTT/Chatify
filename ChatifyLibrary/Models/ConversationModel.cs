namespace ChatifyLibrary.Models;

public class ConversationModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public List<BasicUserModel> Participants { get; set; } = new();
    public bool IsGroupChat { get; set; }
    public string GroupName { get; set; }
    public bool Archived { get; set; }
}
