using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Application.Models
{
    public class User
    {
        [Key]
        [Required]
        [JsonPropertyName("user_id")] // Optional: JSON naming in Swagger
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,16}$",
            ErrorMessage = "Password must be 6 to 16 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        [DataType(DataType.Password)]
        [JsonPropertyName("PasswordHash")]
        [SwaggerSchema(Format = "PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        [Required]
        [JsonPropertyName("role")]
        public Role Role { get; set; } = new Role();
    }
}

