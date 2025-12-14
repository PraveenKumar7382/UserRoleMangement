namespace UserRoleMangement.Database.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using UserRoleMangement.Database.Repositories.Interfaces;
    using UserRoleMangement.Models;

    public class RoleRepository: IRoleRepository
    {
        private readonly DatabaseContext _context;

        public RoleRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetById(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<(bool IsExixtingRole, int Id)> GetByName(string roleName)
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(x=>x.RoleName == roleName);
            if (existingRole != null)                return (true, existingRole.RoleId);

            return (false, 0);
        }

        public async Task<Role> AddAsync(Role role)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role.RoleName);

            if (existingRole != null)
            {
                role.RoleId = existingRole.RoleId;
            }
            else
            {
                var newRole = new Role { RoleName = role.RoleName, Description = role.Description };
                _context.Roles.Add(newRole);
                await _context.SaveChangesAsync();

            }
            return role;
        }
    }
}
