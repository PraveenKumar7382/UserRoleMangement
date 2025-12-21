using Application.Models;

namespace Application.Database.Repositories.Interfaces
{
    public interface IUserSessionRepository
    {
        Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
        Task<UserSession?> GetActiveSessionForUserAsync(int userId);
        Task CreateAsync(UserSession session);
        Task RevokeAsync(string refreshToken);
        Task DeleteSessionByGuidAsync(Guid userId);
        Task DeleteSessionAsync(int userId);
        Task UpdateSessionAsync(UserSession session);
        Task ReplaceAsync(string oldToken, string newToken, DateTime expiresAt);
    }
}
