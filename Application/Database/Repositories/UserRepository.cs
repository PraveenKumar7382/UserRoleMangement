using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using Application.Database.Repositories.Interfaces;
using Application.Models;

namespace Application.Database.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;
        private readonly IRoleRepository _roleRepository;
        private readonly IStringLocalizer<UserRepository> _localizer;

        public UserRepository(DatabaseContext context, IRoleRepository roleRepository, IStringLocalizer<UserRepository> localizer)
        {
            _context = context;
            _roleRepository = roleRepository;
            _localizer = localizer;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Include(u => u.Role).ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            var result = await _context.Users
                               .Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.UserId == id);
            return result;
        }

        public async Task<(bool IsSuccess, string Message, User? User)> AddAsync(User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email && u.UserName == user.UserName);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            if (existingUser != null)
            {
                return (false, _localizer["UserAlreadyExists"], null);
            }
            var existingRole = await _roleRepository.GetByName(user.Role!.RoleName!);

            if (existingRole.IsExixtingRole)
            {
                user.RoleId = existingRole.Id;
                user.Role = null;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, _localizer["UserAddedSuccessfully"], user);
        }

        public async Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPassword forgotPassword)
        {
            User existingUser = await _context.Users.Include(u => u.Role).Where(u => u.UserName == forgotPassword.UserName).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return (false, _localizer["UserNameNotExist"]);
            }

            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(forgotPassword.NewPassword);

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return (true, _localizer["PasswordUpdated"]);
        }

        public async Task<(bool IsSuccess, string Message, User? User)> LoginAsync(Login login)
        {
            var users = await _context.Users.Include(u=>u.Role).Where(u => u.UserName == login.UserName).ToListAsync();

            if (users == null || users.Count == 0)
                return (false, _localizer["InvalidLogin"], null);

            foreach (var user in users)
            {
                if (BCrypt.Net.BCrypt.Verify(login.PasswordHash, user.PasswordHash))
                {
                    return (true, _localizer["LoginSuccessful"], user);
                }
            }
            return (false, _localizer["InvalidLogin"], null);
        }

        public async Task<User?> UpdateAsync(User user)
        {
            var trackedEntity = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (trackedEntity == null)
                return null;

            bool isChanged = false;

            if (trackedEntity.UserName != user.UserName)
            {
                trackedEntity.UserName = user.UserName;
                isChanged = true;
            }

            if (trackedEntity.Email != user.Email)
            {
                trackedEntity.Email = user.Email;
                isChanged = true;
            }

            if (!string.IsNullOrEmpty(user.PasswordHash) &&
                !BCrypt.Net.BCrypt.Verify(user.PasswordHash, trackedEntity.PasswordHash))
            {
                trackedEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                isChanged = true;
            }

            if (user.Role != null)
            {
                var role = await _roleRepository.AddAsync(user.Role);
                if (trackedEntity.RoleId != role.RoleId)
                {
                    trackedEntity.RoleId = role.RoleId;
                    isChanged = true;
                }
            }

            if (!isChanged)
                return null;

            await _context.SaveChangesAsync();
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return false;

            var role = user.Role;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            bool roleUsed = await _context.Users.AnyAsync(u => u.RoleId == role.RoleId);

            if (!roleUsed)
            {
                _context.Roles.Remove(role!);
                await _context.SaveChangesAsync();
            }

            bool IsAnySessionAvailble = await _context.UserSessions.AnyAsync(s => s.UserId == userId);

            if (IsAnySessionAvailble)
            {
              _context.UserSessions.RemoveRange(_context.UserSessions.Where(s => s.UserId == userId));
              await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}

