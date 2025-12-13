using System.ComponentModel.DataAnnotations;

namespace UserRoleMangement.Models
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(@"^[0-9 ]+$",
                ErrorMessage = "Username can contain only digits and spaces.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(16, MinimumLength = 6,
            ErrorMessage = "Password must be between 6 and 16 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,16}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}