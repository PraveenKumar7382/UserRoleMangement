namespace UserRoleMangement.Database.Repositories.Interfaces
{
    using UserRoleMangement.Models;
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetById(int id);
        Task<(bool IsSuccess, string Message, User? User)> AddAsync(User user);
        Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPassword forgotPassword);
        Task<(bool IsSuccess, string Message, User? User)> LoginAsync(Login login);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }
}
