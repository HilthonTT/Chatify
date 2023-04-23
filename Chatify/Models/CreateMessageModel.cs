using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateMessageModel
{
    [Required(ErrorMessage = "You must enter text.")]
    [MaxLength(256)]
    public string Text { get; set; }
}
