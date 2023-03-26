using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateFriendRequestModel
{
    [Required]
    [MinLength(1)]
    [Display(Name = "Friend Code")]
    public string FriendCode { get; set; }
}
