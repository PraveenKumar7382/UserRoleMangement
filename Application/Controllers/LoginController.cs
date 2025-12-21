using Application.Database.Repositories.Interfaces;
using Application.Models;
using Application.TokenGeneration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Cryptography;
using System.Security.Claims;

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

        [HttpPost("Login")]
        public async Task<IActionResult> Login(Login login)
        {
            var result = await _userRepo.LoginAsync(login);
            if (!result.IsSuccess || result.User == null)
                return Unauthorized(new { message = _localizer["InvalidCredentials"] });

            var activeSession = await _sessionRepo.GetActiveSessionForUserAsync(result.User.UserId);
            if (activeSession != null)
                return Unauthorized(new { message = _localizer["UserAlreadyLoggedIn"] });


            var refreshToken = GenerateRefreshToken();
            var sessionId = Guid.NewGuid();
            
            await _sessionRepo.CreateAsync(new UserSession
            {
                Id = sessionId,
                UserId = result.User.UserId,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                Revoked = false,
                CreatedAt = DateTime.Now
            });

            var accessToken = _jwtTokenHelper.GenerateToken(result.User, sessionId);
            SetRefreshTokenCookie(sessionId, refreshToken);

            return Ok(new
            {
                message = _localizer["LoginSuccess"],
                accessToken,
                expiresInMinutes = _configuration["Jwt:ExpiryMinutes"]
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var sessionId = GetSessionIdFromJwt();
            var cookieName = GetRefreshTokenCookieName(sessionId);

            if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken))
                return Unauthorized(new { message = _localizer["SessionNotFound"] });

            var session = await _sessionRepo.GetByRefreshTokenAsync(refreshToken);
            if (session == null || session.ExpiresAt < DateTime.Now)
                return Unauthorized(new { message = _localizer["SessionExpiredOrLoggedOut"] });

            var user = await _userRepo.GetById(session.UserId);
            if (user == null)
                return Unauthorized();

            var newRefreshToken = GenerateRefreshToken();

            await _sessionRepo.ReplaceAsync(
                refreshToken,
                newRefreshToken,
                DateTime.Now.AddMinutes(20)
            );

            SetRefreshTokenCookie(sessionId, newRefreshToken);

            var newAccessToken = _jwtTokenHelper.GenerateToken(user, sessionId);

            return Ok(new
            {
                message = _localizer["TokenRefreshed"],
                accessToken = newAccessToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var sessionId = GetSessionIdFromJwt();
            var cookieName = GetRefreshTokenCookieName(sessionId);

            await _sessionRepo.DeleteSessionByGuidAsync(sessionId);
            Response.Cookies.Delete(cookieName);
          
            return Ok(new { message = _localizer["LogoutSuccess"] });
        }

        private Guid GetSessionIdFromJwt()
        {
            var claim = HttpContext.Items["sessionId"]?.ToString();
            return Guid.Parse(claim!);
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
                SameSite = SameSiteMode.None, 
                Expires = DateTime.Now.AddMinutes(5)
            });
        }

        private string GetRefreshTokenCookieName(Guid guid) => $"refresh_{guid}";
    }
}
