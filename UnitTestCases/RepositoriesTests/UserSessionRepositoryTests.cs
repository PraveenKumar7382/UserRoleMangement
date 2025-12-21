using Application.Database;
using Application.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTestCases.RepositoriesTests
{
    public class UserSessionRepositoryTests
    {
        private DatabaseContext _context = null!;
        private UserSessionRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new DatabaseContext(options);
            _repository = new UserSessionRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateAsync_AddsSessionToDb()
        {
            var session = new UserSession { UserId = 1, RefreshToken = "token1", ExpiresAt = DateTime.Now.AddMinutes(10) };

            await _repository.CreateAsync(session);

            var stored = await _context.UserSessions.FirstOrDefaultAsync();
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.RefreshToken, Is.EqualTo("token1"));
        }

        [Test]
        public async Task GetByRefreshTokenAsync_ReturnsActiveSession()
        {
            var session = new UserSession { UserId = 1, RefreshToken = "token1", ExpiresAt = DateTime.Now.AddMinutes(10), Revoked = false };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByRefreshTokenAsync("token1");

            Assert.That(result, Is.Not.Null);
            Assert.That(session.UserId, Is.EqualTo( result!.UserId));
        }

        [Test]
        public async Task GetByRefreshTokenAsync_RevokedSession_ReturnsNull()
        {
            var session = new UserSession { UserId = 1, RefreshToken = "token1", ExpiresAt = DateTime.Now.AddMinutes(10), Revoked = true };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByRefreshTokenAsync("token1");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllActiveSessionsForUserAsync_ReturnsOnlyActiveSessions()
        {
            var active1 = new UserSession { UserId = 1, RefreshToken = "a1", ExpiresAt = DateTime.Now.AddMinutes(10), Revoked = false };
            var active2 = new UserSession { UserId = 1, RefreshToken = "a2", ExpiresAt = DateTime.Now.AddMinutes(5), Revoked = false };
            var expired = new UserSession { UserId = 1, RefreshToken = "exp", ExpiresAt = DateTime.Now.AddMinutes(-5), Revoked = false };
            _context.UserSessions.AddRange(active1, active2, expired);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllActiveSessionsForUserAsync(1);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ReplaceAsync_UpdatesSessionTokenAndExpiry()
        {
            var session = new UserSession { UserId = 1, RefreshToken = "old", ExpiresAt = DateTime.Now.AddMinutes(10) };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            await _repository.ReplaceAsync("old", "new", DateTime.Now.AddMinutes(20));

            var updated = await _context.UserSessions.FirstAsync();
            Assert.That(updated.RefreshToken, Is.EqualTo("new"));
            Assert.That(updated.ExpiresAt, Is.GreaterThan(DateTime.Now.AddMinutes(19)));
        }

        [Test]
        public async Task DeleteSessionAsync_RemovesAllUserSessions()
        {
            _context.UserSessions.AddRange(
                new UserSession { UserId = 1, RefreshToken = "t1" },
                new UserSession { UserId = 1, RefreshToken = "t2" }
            );
            await _context.SaveChangesAsync();

            await _repository.DeleteSessionAsync(1);

            Assert.That(await _context.UserSessions.Where(s => s.UserId == 1).ToListAsync(), Is.Empty);
        }

        [Test]
        public async Task RevokeAsync_SetsRevokedTrue()
        {
            var session = new UserSession { UserId = 1, RefreshToken = "token", Revoked = false };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            await _repository.RevokeAsync("token");

            var updated = await _context.UserSessions.FirstAsync();
            Assert.That(updated.Revoked, Is.True);
        }

        [Test]
        public async Task DeleteSessionByGuidAsync_SetsRevokedForMatchingSessions()
        {
            var guid = Guid.NewGuid();
            var session = new UserSession { Id = guid, UserId = 1, RefreshToken = "token", Revoked = false };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            await _repository.DeleteSessionByGuidAsync(guid);

            var updated = await _context.UserSessions.FirstAsync();
            Assert.That(updated.Revoked, Is.True);
        }
    }
}
