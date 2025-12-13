using System.ComponentModel.DataAnnotations;

namespace UserRoleMangement.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        [Required(ErrorMessage = "RoleName is mandatory.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "RoleName must be between 2 and 100 characters.")]
        [RegularExpression("^[a-zA-Z0-9_\\- ]+$", ErrorMessage = "RoleName can contain letters, digits, spaces, hyphens (-), and underscores (_).")]
        public string? RoleName { get; set; }
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = [];
    }
}
