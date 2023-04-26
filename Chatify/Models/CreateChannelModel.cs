using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateChannelModel
{
    [Required]
    [Display(Name = "Channel Name")]
    [MaxLength(100)]
    public string ChannelName { get; set; }

    [Required]
    [Display(Name = "Channel Description")]
    [MaxLength(255)]
    public string ChannelDescription { get; set; }
}
