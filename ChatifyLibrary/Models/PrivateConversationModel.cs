namespace ChatifyLibrary.Models;

public class PrivateConversationModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel FirstParticipant { get; set; }
    public BasicUserModel LastParticipant { get; set;  }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }

    public string GroupName
    {
        get
        {
            return $"{ FirstParticipant.FullName } and { LastParticipant.FullName }";
        }
    }
}
