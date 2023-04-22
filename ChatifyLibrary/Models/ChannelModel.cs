namespace ChatifyLibrary.Models;

public class ChannelModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }
    public string ChannelName { get; set; }
    public string ChannelDescription { get; set; }
    public List<MessageModel> Messages { get; set; } = new();
    public List<RoleModel> AllowedRoles { get; set; } = new();
    public List<RoleModel> DisallowedRoles { get; set; } = new();
    public ServerModel Server { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }
}
