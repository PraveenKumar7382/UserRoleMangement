using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Models;

namespace Application.TokenGeneration
{
    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<JwtTokenHelper> _localizer;

        public JwtTokenHelper(IConfiguration configuration, IStringLocalizer<JwtTokenHelper> localizer)
        {
            _configuration = configuration;
            _localizer = localizer;
        }

        public string GenerateToken(User user, Guid sessionId)
        {
            try
            {
                var claims = new[]
                {
                  new Claim("userId", user.UserId.ToString()),
                  new Claim(ClaimTypes.Role, user.Role!.RoleName),
                  new Claim(ClaimTypes.Name, user.UserName),
                  new Claim("sessionId", sessionId.ToString())
                };

                var keyString = _configuration["Jwt:Key"];
                var keyBytes = Encoding.UTF8.GetBytes(keyString);

                if (keyBytes.Length < 32)
                {
                    using var sha256 = SHA256.Create();
                    keyBytes = sha256.ComputeHash(keyBytes);
                }

                var key = new SymmetricSecurityKey(keyBytes);

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "3")
                    ),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(_localizer["JwtGenerationFailed"], ex);
            }
        }
    }
}
