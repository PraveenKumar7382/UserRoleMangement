using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        [Required(ErrorMessage = "RoleName is mandatory.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "RoleName must be between 2 and 100 characters.")]
        public string? RoleName { get; set; }
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }
    }
}
