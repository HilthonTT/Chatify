namespace ChatifyLibrary.Models;

public class ServerInvitationModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public ServerModel Server { get; set; }
    public BasicUserModel User { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string InvitationCode { get; set; }
}
