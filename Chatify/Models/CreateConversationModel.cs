using ChatifyLibrary.BasicModel;
using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateConversationModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Group Name")]
    public string GroupName { get; set; }

    [Required]
    [MinLength(1)]
    [Display(Name = "Category")]
    public string CategoryId { get; set; }

    [Display(Name = "Picture Name")]
    public string PictureName { get; set; }

    [Required]
    public List<BasicUserModel> Participants { get; set; } = new();
}
