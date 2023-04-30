namespace ChatifyLibrary.BasicModel;
public class BasicConversationModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Owner { get; set; }
    public string GroupName { get; set; }
    public string PictureName { get; set; }
    public string OriginalPictureName { get; set; }
    public string PictureExtension { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public BasicConversationModel()
    {
        
    }

    public BasicConversationModel(ConversationModel conversation)
    {
        Id = conversation.Id;
        Owner = conversation.Owner;
        GroupName = conversation.GroupName;
        PictureName = conversation.PictureName;
        OriginalPictureName = conversation.OriginalPictureName;
        PictureExtension = conversation.PictureExtension;
        DateCreated = conversation.DateCreated;
    }
}
