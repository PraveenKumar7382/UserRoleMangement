using Application.Database.Repositories.Interfaces;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UnitTestCases.MiddlewareTests
{
    public class SessionAuthMiddlewareTests
    {
        private Mock<IStringLocalizer<SessionAuthMiddleware>> _localizerMock = null!;
        private Mock<IUserSessionRepository> _userSessionRepoMock = null!;

        [SetUp]
        public void Setup()
        {
            _localizerMock = new Mock<IStringLocalizer<SessionAuthMiddleware>>();
            _userSessionRepoMock = new Mock<IUserSessionRepository>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));
        }

        private static DefaultHttpContext CreateHttpContext(string path = "/api/test", string token = null)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Request.Path = path;
            if (!string.IsNullOrEmpty(token))
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            return context;
        }

        private static async Task<string> ReadResponse(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(context.Response.Body).ReadToEndAsync();
        }

        private static string GenerateJwt(int userId, string role)
        {
            var claims = new[] { new Claim("userId", userId.ToString()), new Claim(ClaimTypes.Role, role), new Claim("sessionId", Guid.NewGuid().ToString()) };
            var token = new JwtSecurityToken(claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Test]
        public async Task InvokeAsync_ExcludedPath_AllowsRequest()
        {
            var context = CreateHttpContext("/swagger");
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task InvokeAsync_MissingAuthorizationHeader_ReturnsUnauthorized()
        {
            var context = CreateHttpContext();
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("AuthTokenMissing"));
        }

        [Test]
        public async Task InvokeAsync_InvalidToken_ReturnsUnauthorized()
        {
            var context = CreateHttpContext(token: "invalidtoken");
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("InvalidToken"));
        }

        [Test]
        public async Task InvokeAsync_NoActiveSession_ReturnsUnauthorized()
        {
            var token = GenerateJwt(1, "Admin");
            var context = CreateHttpContext(token: token);
            _userSessionRepoMock.Setup(r => r.GetActiveSessionForUserAsync(1)).ReturnsAsync((UserSession)null);
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("SessionExpiredOrLoggedOut"));
        }

        [Test]
        public async Task InvokeAsync_ExpiredSession_ReturnsUnauthorizedAndDeletesSession()
        {
            var token = GenerateJwt(1, "Admin");
            var context = CreateHttpContext(token: token);
            _userSessionRepoMock.Setup(r => r.GetActiveSessionForUserAsync(1))
                .ReturnsAsync(new UserSession { UserId = 1, ExpiresAt = DateTime.Now.AddMinutes(-1), Revoked = false });
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            _userSessionRepoMock.Verify(r => r.DeleteSessionAsync(1), Times.Once);
            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("SessionExpired"));
        }

        [Test]
        public async Task InvokeAsync_RevokedSession_ReturnsUnauthorized()
        {
            var token = GenerateJwt(1, "Admin");
            var context = CreateHttpContext(token: token);
            _userSessionRepoMock.Setup(r => r.GetActiveSessionForUserAsync(1))
                .ReturnsAsync(new UserSession { UserId = 1, ExpiresAt = DateTime.Now.AddMinutes(10), Revoked = true });
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("AlreadyLoggedInElsewhere"));
        }

        [Test]
        public async Task InvokeAsync_ValidSession_SetsContextItems()
        {
            var token = GenerateJwt(1, "Admin");
            var context = CreateHttpContext(token: token);
            _userSessionRepoMock.Setup(r => r.GetActiveSessionForUserAsync(1))
                .ReturnsAsync(new UserSession { UserId = 1, ExpiresAt = DateTime.Now.AddMinutes(10), Revoked = false });
            var middleware = new SessionAuthMiddleware(_ => Task.CompletedTask, _localizerMock.Object);

            await middleware.InvokeAsync(context, _userSessionRepoMock.Object);

            Assert.That(context.Items["UserId"], Is.EqualTo(1));
            Assert.That(context.Items["Role"], Is.EqualTo("Admin"));
        }
    }
}
