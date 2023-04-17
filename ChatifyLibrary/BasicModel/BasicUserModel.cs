namespace ChatifyLibrary.BasicModel;

public class BasicUserModel
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string FileName { get; set; }
    public DateTime DateCreated { get; set; }

    public BasicUserModel()
    {
        
    }
    
    public BasicUserModel(UserModel user)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        DateCreated = user.DateCreated;
        DisplayName = user.DisplayName;
        FileName = user.FileName;
    }

    public string FullName
    {
        get
        {
            return $" { FirstName } { LastName }";
        }
    }
}
