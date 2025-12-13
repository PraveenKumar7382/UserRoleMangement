using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.Models;

namespace UserRoleMangement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public LoginController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<(bool IsSuccess, string Message, string? Token)> Login(Login login)
        {
            var isValidUser = await _userRepository.LoginAsync(login);

            if (!isValidUser.IsSuccess)
                return (false, isValidUser.Message, null);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                  new Claim(ClaimTypes.Name, isValidUser.User!.UserName),
                  new Claim(ClaimTypes.NameIdentifier, isValidUser.User.UserId.ToString())
                ]),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            isValidUser.User.CurrentToken = jwtToken;
            isValidUser.User.TokenExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.UpdateAsync(isValidUser.User);

            return (true, "Login successful", jwtToken);
        }
    }
}