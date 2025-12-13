using UserRoleMangement.Models;

namespace UserRoleMangement.Database.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetById(int id);
        Task<Role> AddAsync(Role role);
    }
}
