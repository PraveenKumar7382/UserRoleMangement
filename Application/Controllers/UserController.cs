namespace Application.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Application.Database.Repositories.Interfaces;
    using Application.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IStringLocalizer<UserController> _localizer;

        public UserController(IUserRepository repo, IStringLocalizer<UserController> localizer)
        {
            _repo = repo;
            _localizer = localizer;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            string role = HttpContext.Items["Role"]?.ToString()!;

            if (role.ToLower() != "admin")
                return Forbid(_localizer["AccessDenied"]);

            List<User> users = [.. (await _repo.GetAllAsync())];

            var userDtos = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role?.RoleName,
                RoleDescription = u.Role?.Description
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            int userId = (int)HttpContext.Items["UserId"]!;
            return Ok(await _repo.GetById(userId));
        }

        [HttpPut("me")]
        public async Task<IActionResult> Update(User user)
        {
            int userId = (int)HttpContext.Items["UserId"]!;
            user.UserId = userId;
            var result = await _repo.UpdateAsync(user);

            if (result == null)
            {
                return Ok(new
                {
                    message = _localizer["NoChanges"]
                });
            }

            return Ok(new
            {
                message = _localizer["UserUpdated"],
                result.UserId,
                result.UserName,
                result.Email,
                result.Role?.RoleName,
                result.Role?.Description
            });
        }

        [HttpDelete("me")]
        public async Task<IActionResult> Delete()
        {
            int userId = (int)HttpContext.Items["UserId"]!;
            await _repo.DeleteAsync(userId);

            return Ok(new
            {
                message = _localizer["UserDeleted"],
            });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(User user)
        {
            var result = await _repo.AddAsync(user);

            if (!result.IsSuccess)
                return BadRequest(new { message = _localizer[result.Message] });

            return Ok(new
            {
                message = _localizer["UserCreated"],
                result.User!.UserId,
                result.User!.UserName,
                result.User.Email,
                result.User.Role?.RoleName,
                result.User.Role?.Description
            });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword forgotPassword)
        {
            var result = await _repo.ForgotPasswordAsync(forgotPassword);

            if (!result.IsSuccess)
                return BadRequest(new { message = _localizer[result.Message] });

            return Ok(new
            {
                message = _localizer["PasswordResetSuccess"]
            });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            string role = HttpContext.Items["Role"]?.ToString()!;

            if (role.ToLower() != "admin")
                return Forbid(_localizer["AccessDenied"]);

            user.UserId = id;
            var updated = await _repo.UpdateAsync(user);

            if (updated == null)
                return Ok(new { message = _localizer["NoChanges"] });

            return Ok(new
            {
                message = _localizer["UserUpdated"],
                updated.UserId,
                updated.UserName,
                updated.Email,
                Role = updated.Role?.RoleName
            });
        }


        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string role = HttpContext.Items["Role"]?.ToString()!;

            if (role.ToLower() != "admin")
                return Forbid(_localizer["AccessDenied"]);

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = _localizer["UserNotFound"] });

            return Ok(new
            {
                message = _localizer["UserDeleted"],
                UserId = id
            });
        }
    }
}
