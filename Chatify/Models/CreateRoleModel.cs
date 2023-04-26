using System.ComponentModel.DataAnnotations;

namespace Chatify.Models;

public class CreateRoleModel
{
    [Required]
    [Display(Name = "Role Name")]
    [MaxLength(100)]
    public string RoleName { get; set; }

    [Required]
    [Display(Name = "Role Description")]
    [MaxLength(255)]
    public string RoleDescription { get; set; }

    [Required]
    [Display(Name = "Can Ban Member")]
    public bool CanBanMember { get; set; }

    [Required]
    [Display(Name = "Can Kick Member")]
    public bool CanKickMember { get; set; } = false;

    [Required]
    [Display(Name = "Can Create Channel")]
    public bool CanCreateChannel { get; set; } = false;

    [Required]
    [Display(Name = "Can Create Role")]
    public bool CanCreateRole { get; set; } = false;

    [Required]
    [Display(Name = "Can Give Member")]
    public bool CanGiveRole { get; set; } = false;

    [Required]
    [Display(Name = "Can View Audit Log")]
    public bool CanViewAuditLog { get; set; }

    [Required]
    [Display(Name = "Can Edit Server")]
    public bool CanEditServer { get; set; }
}
