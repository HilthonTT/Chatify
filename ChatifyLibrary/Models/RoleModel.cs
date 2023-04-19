namespace ChatifyLibrary.Models;

public class RoleModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string RoleName { get; set; }
    public string RoleDescription { get; set; }
    public ServerModel Server { get; set; }
    public List<BasicUserModel> Users { get; set; } = new();
    public bool CanBanMember { get; set; }
    public bool CanKickMember { get; set; }
    public bool CanCreateChannel { get; set; }
    public bool CanCreateRole { get; set; }
    public bool CanGiveRole { get; set; }
    public bool CanViewAuditLog { get; set; }
    public bool CanEditServer { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }
}
