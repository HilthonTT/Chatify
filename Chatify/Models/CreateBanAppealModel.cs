using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateBanAppealModel
{
    [Required]
    [Display(Name = "Appeal Reason")]
    public string AppealReason { get; set; }
}
