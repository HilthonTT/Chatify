namespace ChatifyLibrary.Models;

public class UserModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<BasicUserModel> Friends { get; set; } = new();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
