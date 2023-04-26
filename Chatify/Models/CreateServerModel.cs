using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateServerModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Server Name")]
    public string ServerName { get; set; }

    [Required]
    [MaxLength(255)]
    [Display(Name = "Server Description")]
    public string ServerDescription { get; set; }

    [Display(Name = "Picture Name")]
    public string PictureName { get; set; }

    [Required]
    [MinLength(1)]
    [Display(Name = "Category")]
    public string CategoryId { get; set; }
}