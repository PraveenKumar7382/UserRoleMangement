using System.ComponentModel.DataAnnotations;

namespace UserRoleMangement.Models
{
    public class Login
    {
        [Required(ErrorMessage = "UserName is mandatory.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "UserName must be at least 2 characters long.")]
        [RegularExpression("^[a-zA-Z0-9 ]+$", ErrorMessage = "UserName can contain only letters, digits, and spaces.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is mandatory.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,16}$",
         ErrorMessage = "Password must be 6 to 16 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string PasswordHash { get; set; }
    }
}