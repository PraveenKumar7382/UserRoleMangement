using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class Login
    {
        [Required(ErrorMessage = "UserName is mandatory.")]
        public string UserName { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,16}$",
            ErrorMessage = "Password must be 6 to 16 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        [DataType(DataType.Password)] 
        [SwaggerSchema(Format = "PasswordHash")]
        [JsonPropertyName("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;
    }
}