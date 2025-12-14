using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using UserRoleMangement.Database;
using UserRoleMangement.Database.Repositories;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.Models;

namespace UnitTestCases.Repositories
{
    public class UserControllersTests
    {
        private DatabaseContext _context = null!;
        private UserRepository _repository = null!;
        private Mock<IRoleRepository> _roleRepoMock = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase("UserDb")
                .Options;

            _context = new DatabaseContext(options);
            _roleRepoMock = new Mock<IRoleRepository>();

            _repository = new UserRepository(_context, _roleRepoMock.Object);
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
            _context.Users.Add(new User { UserName = "Admin", Email = "a@test.com", PasswordHash = "pwd" });
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetById_WhenUserExists_ReturnsUser()
        {
            var user = new User { UserName = "Admin", Email = "a@test.com", PasswordHash = "pwd" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetById(user.UserId);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.UserName, Is.EqualTo( result!.UserName));
        }

        [Test]
        public async Task AddAsync_WhenUserAlreadyExists_ReturnsFailure()
        {
            var user = new User
            {
                UserName = "Admin",
                Email = "a@test.com",
                PasswordHash = "pwd",
                Role = new Role { RoleName = "Admin" }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.AddAsync(user);

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task AddAsync_WhenRoleExists_AssignsRoleId()
        {
            var role = new Role { RoleId = 1, RoleName = "Admin" };

            _roleRepoMock
                .Setup(r => r.GetByName("Admin"))
                .ReturnsAsync((true, 1));

            var user = new User
            {
                UserName = "Admin",
                Email = "admin@test.com",
                PasswordHash = "pwd",
                Role = role
            };

            var result = await _repository.AddAsync(user);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(1, Is.EqualTo(result.User!.RoleId));
        }

        [Test]
        public async Task LoginAsync_WhenCredentialsValid_ReturnsSuccess()
        {
            var password = "password";
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Users.Add(new User
            {
                UserName = "admin",
                Email = "a@test.com",
                PasswordHash = hashed
            });
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

            _context.Users.Add(new User
            {
                UserName = "admin",
                Email = "a@test.com",
                PasswordHash = hashed
            });
            await _context.SaveChangesAsync();

            var login = new Login { UserName = "admin", PasswordHash = "wrong" };

            var result = await _repository.LoginAsync(login);

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task UpdateAsync_WhenUserExists_ReturnsUpdatedUser()
        {
            var role = new Role { RoleId = 1, RoleName = "Admin" };

            _roleRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Role>()))
                .ReturnsAsync(role);

            var user = new User
            {
                UserName = "old",
                Email = "a@test.com",
                PasswordHash = "pwd",
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.UserName = "updated";

            var result = await _repository.UpdateAsync(user);

            Assert.That(result, Is.Not.Null);
            Assert.That(user.UserName, Is.EqualTo(result!.UserName));
        }

        [Test]
        public async Task DeleteAsync_WhenUserExists_ReturnsTrue()
        {
            var role = new Role { RoleName = "Admin" };
            var user = new User
            {
                UserName = "admin",
                Email = "a@test.com",
                PasswordHash = "pwd",
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            bool result = await _repository.DeleteAsync(user.UserId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
        {
            var result = await _repository.DeleteAsync(999);

            Assert.IsFalse(result);
        }
    }
}
