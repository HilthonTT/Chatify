namespace ChatifyLibrary.BasicModel;
public class BasicServerModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public BasicUserModel Owner { get; set; }
    public CategoryModel Category { get; set; }
    public string ServerName { get; set; }
    public string ServerDescription { get; set; }
    public string PictureName { get; set; }
    public string OriginalPictureName { get; set; }
    public string PictureExtension { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public BasicServerModel()
    {
        
    }

    public BasicServerModel(ServerModel server)
    {
        Id = server.Id;
        Owner = server.Owner;
        Category = server.Category;
        ServerName = server.ServerName;
        ServerDescription = server.ServerDescription;
        PictureName = server.PictureName;
        OriginalPictureName = server.OriginalPictureName;
        PictureExtension = server.PictureExtension;
        DateCreated = server.DateCreated;
    }
}
