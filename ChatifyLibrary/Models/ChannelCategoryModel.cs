namespace ChatifyLibrary.Models;
public class ChannelCategoryModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
    public ServerModel Server { get; set; }
    public List<ChannelModel> Channels { get; set; } = new();
    public bool Archived { get; set; }
}
