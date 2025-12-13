namespace UserRoleMangement.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using UserRoleMangement.Database.Repositories.Interfaces;
    using UserRoleMangement.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _repo;

        public RoleController(IRoleRepository repo)
        {
            _repo = repo;
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
            if (role == null) return NotFound("Role not found");

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.AddAsync(role);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            var existing = await _repo.GetById(id);
            if (existing == null)
                return NotFound("Role not found");

            existing.RoleName = role.RoleName;
            existing.Description = role.Description;

            var result = await _repo.AddAsync(existing);
            return Ok(result);
        }
    }
}
