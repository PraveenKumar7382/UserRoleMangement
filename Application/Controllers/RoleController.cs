namespace Application.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Application.Database.Repositories.Interfaces;
    using Application.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _repo;
        private readonly IStringLocalizer<RoleController> _localizer;

        public RoleController(IRoleRepository repo, IStringLocalizer<RoleController> localizer)
        {
            _repo = repo;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _repo.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _repo.GetById(id);
            if (role == null) return NotFound(new { message = _localizer["RoleNotFound"] });

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exists, id) = await _repo.GetByName(role.RoleName!);

            if (exists)
                return Conflict(new { message = _localizer["RoleAlreadyExists"] });

            var result = await _repo.AddAsync(role);
            return Ok(new
            {
                message = _localizer["RoleCreatedSuccessfully"],
                data = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            var existing = await _repo.GetById(id);
            if (existing == null)
                return NotFound(new { message = _localizer["RoleNotFound"] });

            var result = await _repo.AddAsync(role);
            return Ok(new
            {
                message = _localizer["RoleUpdatedSuccessfully"],
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var existing = await _repo.GetById(id);
            if (existing == null)
                return NotFound(new { message = _localizer["RoleNotFound"] });

            bool isSuccess = await _repo.DeleteAsync(existing);

            if (!isSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    message = _localizer["RoleInUse"]
                });
            }

            return Ok(new
            {
                success = true,
                message = _localizer["RoleDeletedSuccessfully"]
            });
        }
    }
}
