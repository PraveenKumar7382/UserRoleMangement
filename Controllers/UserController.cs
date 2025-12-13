namespace UserRoleMangement.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UserRoleMangement.Database.Repositories.Interfaces;
    using UserRoleMangement.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get() =>
            Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _repo.GetById(id);

            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(User user)
        {
            var result = await _repo.AddAsync(user);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
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
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message
            });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            user.UserId = id;
            var result = await _repo.UpdateAsync(user);

            if(result == null) { return NotFound(); } 

            return Ok(new
            {
                message = "User Datails are updated",
                result.UserId,
                result.UserName,
                result.Email,
                result.Role?.RoleName,
                result.Role?.Description
            });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if(!result){
                  return NotFound();
            }
            return Ok(await _repo.DeleteAsync(id));
        }
    }
}
