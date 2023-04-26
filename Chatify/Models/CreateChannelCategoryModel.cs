using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateChannelCategoryModel
{
    [Required]
    [Display(Name = "Category Name")]
    [MaxLength(100)]
    public string CategoryName { get; set; }

    [Required]
    [Display(Name = "Category Description")]
    [MaxLength(255)]
    public string CategoryDescription { get; set; }
}
