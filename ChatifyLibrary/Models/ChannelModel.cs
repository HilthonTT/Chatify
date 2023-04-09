namespace ChatifyLibrary.Models;

public class ChannelModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ChannelName { get; set; }
    public string ChannelDescription { get; set; }
    public List<MessageModel> Messages { get; set; } = new();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }
}
