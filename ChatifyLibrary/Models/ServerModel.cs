﻿namespace ChatifyLibrary.Models;

public class ServerModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }
    public BasicUserModel Owner { get; set; }
    public List<BasicUserModel> Members { get; set; } = new();
    public List<BasicUserModel> BannedUsers { get; set; } = new();
    public CategoryModel Category { get; set; }
    public string ServerName { get; set; }
    public string ServerDescription { get; set; }
    public string PictureName { get; set; }
    public string OriginalPictureName { get; set; }
    public string PictureExtension { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public bool Archived { get; set; }
}
