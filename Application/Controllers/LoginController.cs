using Application.Database.Repositories.Interfaces;
using Application.Models;
using Application.TokenGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserSessionRepository _sessionRepo;
        private readonly IJwtTokenHelper _jwtTokenHelper;
        private readonly IStringLocalizer<LoginController> _localizer;
        private readonly IConfiguration _configuration;

        public LoginController(
            IUserRepository userRepo,
            IUserSessionRepository sessionRepo,
            IJwtTokenHelper jwtTokenHelper,
            IConfiguration configuration,
            IStringLocalizer<LoginController> localizer)
        {
            _userRepo = userRepo;
            _sessionRepo = sessionRepo;
            _jwtTokenHelper = jwtTokenHelper;
            _configuration = configuration;
            _localizer = localizer;
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            var result = await _userRepo.LoginAsync(login);
            if (!result.IsSuccess || result.User == null)
                return Unauthorized(new { message = _localizer["InvalidCredentials"] });

            var activeSession = await _sessionRepo.GetActiveSessionForUserAsync(result.User.UserId);
            if (activeSession != null)
                return Unauthorized(new { message = _localizer["UserAlreadyLoggedIn"] });

            var accessToken = _jwtTokenHelper.GenerateToken(result.User);
            var refreshToken = GenerateRefreshToken();
            var sessionId = Guid.NewGuid();

            await _sessionRepo.CreateAsync(new UserSession
            {
                Id = sessionId,
                UserId = result.User.UserId,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20),
                Revoked = false,
                CreatedAt = DateTime.UtcNow
            });

            SetRefreshTokenCookie(sessionId, refreshToken);

            var refreshUrl = $"{Request.Scheme}://{Request.Host}/api/login/refresh?sessionId={sessionId}";

            return Ok(new
            {
                message = _localizer["LoginSuccess"],
                accessToken,
                expiresInMinutes = _configuration["Jwt:ExpiryMinutes"],
                refreshTokenUrl = refreshUrl
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromQuery] Guid guid)
        {
            var cookieName = GetRefreshTokenCookieName(guid);
            if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken))
                return Unauthorized(new { message = _localizer["SessionNotFound"] });

            var session = await _sessionRepo.GetByRefreshTokenAsync(refreshToken);
            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                await _sessionRepo.DeleteSessionAsync(session?.UserId ?? 0);
                return Unauthorized(new { message = _localizer["SessionExpiredOrLoggedOut"] });
            }

            var user = await _userRepo.GetById(session.UserId);
            if (user == null)
                return Unauthorized();

            var newAccessToken = _jwtTokenHelper.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            await _sessionRepo.ReplaceAsync(refreshToken, newRefreshToken, DateTime.UtcNow.AddMinutes(20));
            SetRefreshTokenCookie(guid, newRefreshToken);

            var refreshUrl = $"{Request.Scheme}://{Request.Host}/api/login/refresh?sessionId={guid}";

            return Ok(new
            {
                message = _localizer["TokenRefreshed"],
                accessToken = newAccessToken,
                expiresInMinutes = _configuration["Jwt:ExpiryMinutes"],
                refreshTokenUrl = refreshUrl
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] Guid guid)
        {
            var cookieName = GetRefreshTokenCookieName(guid);
            if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken))
                return Unauthorized(new { message = _localizer["SessionNotFound"] });

            var session = await _sessionRepo.GetByRefreshTokenAsync(refreshToken);
            if (session == null)
                return Unauthorized(new { message = _localizer["InvalidRefreshToken"] });

            var userId = session.UserId;
            await _sessionRepo.DeleteSessionAsync(userId);
            Response.Cookies.Delete(cookieName);

            return Ok(new { message = _localizer["LogoutSuccess"] });
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private void SetRefreshTokenCookie(Guid guid, string refreshToken)
        {
            Response.Cookies.Append(GetRefreshTokenCookieName(guid), refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(20)
            });
        }

        private string GetRefreshTokenCookieName(Guid guid) => $"refresh_{guid}";
    }
}
