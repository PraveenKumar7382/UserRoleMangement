using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserRoleMangement.Models;

namespace UserRoleMangement.TokenGeneration
{
    public class JwtTokenHelper : IJwtTokenHelper
    {
        private readonly IConfiguration _configuration;

        public JwtTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            try {
                List<Claim> claims =
                [
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
            ];

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
                throw new InvalidOperationException("An error occurred while generating the JWT token.", ex);
            }
        }
    }
}
