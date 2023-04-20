using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class EditingUserRoleModel
{
    [Required]
    [Display(Name = "Role")]
    [MinLength(1)]
    public string RoleId { get; set; }
}
