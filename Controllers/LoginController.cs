using Microsoft.AspNetCore.Mvc;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.Models;
using UserRoleMangement.TokenGeneration;

namespace UserRoleMangement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenHelper _jwtTokenHelper;

        public LoginController(IUserRepository userRepository, IConfiguration configuration, IJwtTokenHelper jwtTokenHelper)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _jwtTokenHelper = jwtTokenHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login login)
        {
            var result = await _userRepository.LoginAsync(login);

            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            var token = _jwtTokenHelper.GenerateToken(result.User!);

            var tokenStore = HttpContext.RequestServices.GetRequiredService<IUserTokenStore>();
            tokenStore.AddToken(result.User!.UserId, token);

            return Ok(new
            {
                message = "Login successful",
                accessToken = token,
                expiresInMinutes = _configuration["Jwt:ExpiryMinutes"]
            });
        }
    }
}