namespace Application.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? RoleName { get; set; }
        public string? RoleDescription { get; set; }
    }
}
