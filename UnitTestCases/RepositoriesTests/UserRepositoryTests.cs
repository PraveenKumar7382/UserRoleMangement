using Application.Database;
using Application.Database.Repositories;
using Application.Database.Repositories.Interfaces;
using Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;

namespace UnitTestCases.Repositories
{
    public class UserRepositoryTests
    {
        private DatabaseContext _context = null!;
        private UserRepository _repository = null!;
        private Mock<IRoleRepository> _roleRepoMock = null!;
        private Mock<IStringLocalizer<UserRepository>> _localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase("UserDb")
                .Options;

            _context = new DatabaseContext(options);

            _roleRepoMock = new Mock<IRoleRepository>();
            _localizerMock = new Mock<IStringLocalizer<UserRepository>>();
            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            _repository = new UserRepository(_context, _roleRepoMock.Object, _localizerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_WhenCalled_ReturnsUsers()
        {
            _context.Users.Add(new User
            {
                UserName = "Admin",
                Email = "a@test.com",
                PasswordHash = "pwd",
                Role = new Role { RoleName = "AdminRole", Description = "Admin Role" }
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetById_WhenUserExists_ReturnsUser()
        {
            var user = new User { UserName = "Admin", Email = "a@test.com", PasswordHash = "pwd", Role = new() { RoleName = "rolename"} };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetById(user.UserId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.UserName, Is.EqualTo(user.UserName));
        }

        [Test]
        public async Task AddAsync_WhenUserAlreadyExists_ReturnsFailure()
        {
            var user = new User { UserName = "Admin", Email = "a@test.com", PasswordHash = "pwd", Role = new Role { RoleName = "Admin" } };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.AddAsync(user);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("UserAlreadyExists"));
        }

        [Test]
        public async Task AddAsync_WhenRoleExists_AssignsRoleId()
        {
            _roleRepoMock.Setup(r => r.GetByName("Admin")).ReturnsAsync((true, 1));

            var user = new User { UserName = "Admin", Email = "admin@test.com", PasswordHash = "pwd", Role = new Role { RoleName = "Admin" } };

            var result = await _repository.AddAsync(user);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.User!.RoleId, Is.EqualTo(1));
            Assert.That(result.User.Role, Is.Null);
        }

        [Test]
        public async Task LoginAsync_WhenCredentialsValid_ReturnsSuccess()
        {
            var password = "password";
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Add(new User { UserName = "admin", Email = "a@test.com", PasswordHash = hashed, Role = new Role { RoleName = "Admin" } });
            await _context.SaveChangesAsync();

            var login = new Login { UserName = "admin", PasswordHash = password };
            var result = await _repository.LoginAsync(login);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.User, Is.Not.Null);
        }

        [Test]
        public async Task LoginAsync_WhenPasswordInvalid_ReturnsFailure()
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword("correct");
            _context.Users.Add(new User { UserName = "admin", Email = "a@test.com", PasswordHash = hashed, Role = new Role { RoleName = "Admin" } });
            await _context.SaveChangesAsync();

            var login = new Login { UserName = "admin", PasswordHash = "wrong" };
            var result = await _repository.LoginAsync(login);

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task DeleteAsync_WhenUserExists_ReturnsTrue()
        {
            var user = new User { UserName = "admin", Email = "a@test.com", PasswordHash = "pwd", Role = new Role { RoleName = "Admin" } };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(user.UserId);

            Assert.That(result, Is.True);
            Assert.That(await _context.Users.AnyAsync(u => u.UserId == user.UserId), Is.False);
        }

        [Test]
        public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            var result = await _repository.DeleteAsync(999);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task ForgotPasswordAsync_WhenUserExists_UpdatesPassword()
        {
            var user = new User { UserName = "user1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpwd"), Role = new Role { RoleName = "Admin" } };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.ForgotPasswordAsync(new ForgotPassword { UserName = "user1", NewPassword = "newpwd" });

            Assert.That(result.IsSuccess, Is.True);
            var updatedUser = await _context.Users.FirstAsync(u => u.UserId == user.UserId);
            Assert.That(BCrypt.Net.BCrypt.Verify("newpwd", updatedUser.PasswordHash), Is.True);
        }

        [Test]
        public async Task ForgotPasswordAsync_WhenUserDoesNotExist_ReturnsFailure()
        {
            var result = await _repository.ForgotPasswordAsync(new ForgotPassword { UserName = "nonexist", NewPassword = "pwd" });
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("UserNameNotExist"));
        }

        [Test]
        public async Task UpdateAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            var user = new User { UserId = 999, UserName = "nonexist", Email = "a@test.com" };
            var result = await _repository.UpdateAsync(user);
            Assert.That(result, Is.Null);
        }
    }
}
