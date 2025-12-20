using System.ComponentModel.DataAnnotations;

public class UserSession
{
    public Guid Id { get; set; }                    

    public int UserId { get; set; }                  

    public string RefreshToken { get; set; } = null!; 
    public DateTime ExpiresAt { get; set; }          
    public bool Revoked { get; set; } = false;      
    public DateTime CreatedAt { get; set; } = DateTime.Now; 
}