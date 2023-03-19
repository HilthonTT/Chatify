namespace ChatifyLibrary.Models;

public class ConversationModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Owner { get; set; }
    public List<BasicUserModel> Participants { get; set; } = new();
    public CategoryModel Category { get; set; }
    public bool IsGroupChat { get; set; }
    public string GroupName { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }
}
