using ChatifyLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateBanModel
{
    [Required]
    [Display(Name = "Banned Until")]
    public DateTime BannedUntil { get; set; }

    [Required]
    public string Reason { get; set; }
}
