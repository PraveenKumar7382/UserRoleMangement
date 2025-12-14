namespace UserRoleMangement.TokenGeneration
{
    public interface IUserTokenStore
    {
        void AddToken(int userId, string token);
        bool IsValidToken(int userId, string token);
        void RemoveToken(int userId, string token);
    }
}
