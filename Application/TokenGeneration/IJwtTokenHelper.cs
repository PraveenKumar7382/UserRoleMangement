namespace Application.TokenGeneration
{
    using Application.Models;
    public interface IJwtTokenHelper
    {
        string GenerateToken(User user, Guid sessionId);
    }
}
