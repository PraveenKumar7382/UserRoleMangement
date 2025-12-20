using Microsoft.EntityFrameworkCore;
using Application.Database.Repositories.Interfaces;
using Application.Models;

namespace Application.Database.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly DatabaseContext _db;

        public UserSessionRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(UserSession session)
        {
            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _db.UserSessions
                .Where(s => s.RefreshToken == refreshToken && !s.Revoked)
                .FirstOrDefaultAsync();
        }

        public async Task<UserSession?> GetActiveSessionForUserAsync(int userId)
        {
            var session = await _db.UserSessions
                .Where(s => s.UserId == userId && !s.Revoked && s.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            var expiredSessions = await _db.UserSessions
                .Where(s => s.UserId == userId && s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredSessions.Any())
            {
                _db.UserSessions.RemoveRange(expiredSessions);
                await _db.SaveChangesAsync();
                return null;
            }

            return session;
        }

        public async Task<List<UserSession>> GetAllActiveSessionsForUserAsync(int userId)
        {
            return await _db.UserSessions
                .Where(s => s.UserId == userId && !s.Revoked && s.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task ReplaceAsync(string oldToken, string newToken, DateTime expiresAt)
        {
            var session = await _db.UserSessions
                .Where(s => s.RefreshToken == oldToken && !s.Revoked)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.RefreshToken = newToken;
                session.ExpiresAt = expiresAt;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteSessionAsync(int userId)
        {
            var sessions = await _db.UserSessions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            if (sessions.Any())
            {
                _db.UserSessions.RemoveRange(sessions);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var session = await _db.UserSessions
                .Where(s => s.RefreshToken == refreshToken && !s.Revoked)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                session.Revoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteSessionByGuidAsync(Guid userId)
        {
            var sessions = await _db.UserSessions
                .Where(s => s.Id == userId && !s.Revoked)
                .ToListAsync();

            if (sessions.Any())
            {
                foreach (var s in sessions)
                    s.Revoked = true;

                await _db.SaveChangesAsync();
            }
        }
    }
}
