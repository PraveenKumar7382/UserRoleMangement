namespace UserRoleMangement.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool Revoked { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public bool IsActive => !Revoked && DateTime.UtcNow <= Expires;
    }
}
