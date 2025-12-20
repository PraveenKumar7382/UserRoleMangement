using Application.Models;

namespace Application.Database.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetById(int id);
        Task<(bool IsExixtingRole, int Id)> GetByName(string roleName);
        Task<Role> AddAsync(Role role);
        Task<bool> DeleteAsync(Role role);
    }
}
