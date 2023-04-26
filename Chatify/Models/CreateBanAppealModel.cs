using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateBanAppealModel
{
    [Required]
    [Display(Name = "Appeal Reason")]
    [MaxLength(255)]
    public string AppealReason { get; set; }
}
