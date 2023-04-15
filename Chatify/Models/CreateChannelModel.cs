using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateChannelModel
{
    [Required]
    [Display(Name = "Channel Name")]
    public string ChannelName { get; set; }
    [Required]
    [Display(Name = "Channel Description")]
    public string ChannelDescription { get; set; }
}
