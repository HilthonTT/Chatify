namespace ChatifyLibrary.Models;

public class BanAppealModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BanModel Ban { get; set; }
    public BasicUserModel AppealingUser { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string AppealReason { get; set; }
    public bool IsApproved { get; set; }
    public BasicUserModel ApprovedAdmin { get; set; }
    public BasicUserModel DisapprovedAdmin { get; set; }
    public DateTime ApprovedAt { get; set; }
    public DateTime DisapprovedAt { get; set; }
}
