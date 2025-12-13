namespace UserRoleMangement.TokenGeneration
{
    using UserRoleMangement.Models;
    public interface IJwtTokenHelper
    {
        string GenerateToken(User user);
    }
}
