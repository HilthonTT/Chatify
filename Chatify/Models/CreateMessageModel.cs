using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateMessageModel
{
    [Required]
    [MaxLength(256)]
    public string Text { get; set; }
}
