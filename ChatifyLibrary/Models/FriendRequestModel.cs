namespace ChatifyLibrary.Models;

public class FriendRequestModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Sender { get; set; }
    public BasicUserModel Receiver { get; set;}
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public bool IsAccepted { get; set; } = false;
}
