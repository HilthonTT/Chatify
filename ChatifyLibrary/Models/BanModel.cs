namespace ChatifyLibrary.Models;

public class BanModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel UserBanned { get; set; }
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    public DateTime BannedUntil { get; set;}
    public string Reason { get; set; }
    public BasicUserModel Admin { get; set; }
}
