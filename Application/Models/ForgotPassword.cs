using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,16}$",
            ErrorMessage = "Password must be 6 to 16 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        [DataType(DataType.Password)]
        [JsonPropertyName("NewPassword")]
        [SwaggerSchema(Format = "NewPassword")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [JsonPropertyName("ConfirmPassword")]
        [SwaggerSchema(Format = "ConfirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}