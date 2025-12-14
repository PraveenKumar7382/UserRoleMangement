using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using UserRoleMangement.Database;
using UserRoleMangement.Database.Repositories;
using UserRoleMangement.Models;

namespace UnitTestCases.Repositories
{
    public class RoleRepositoryTests
    {
        private DatabaseContext _context = null!;
        private RoleRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase("RoleDb")
                .Options;

            _context = new DatabaseContext(options);
            _repository = new RoleRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_WhenCalled_ReturnsAllRoles()
        {
            _context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "User" }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetById_WhenRoleExists_ReturnsRole()
        {
            var role = new Role { RoleName = "Admin" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = await _repository.GetById(role.RoleId);

            Assert.That(result, Is.Not.Null);
            Assert.That(role.RoleName, Is.EqualTo(result!.RoleName));
        }

        [Test]
        public async Task GetByName_WhenRoleExists_ReturnsTrueAndRoleId()
        {
            var role = new Role { RoleName = "Admin" };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByName("Admin");

            Assert.That(result.IsExixtingRole, Is.True); ;
            Assert.That(result.Id, Is.EqualTo(role.RoleId));
        }

        [Test]
        public async Task GetByName_WhenRoleDoesNotExist_ReturnsFalseAndZero()
        {
            var result = await _repository.GetByName("Unknown");

            Assert.That(result.IsExixtingRole, Is.False);
            Assert.That(result.Id, Is.EqualTo(0));
        }

        [Test]
        public async Task AddAsync_WhenRoleDoesNotExist_AddsNewRole()
        {
            var role = new Role { RoleName = "Admin", Description = "Admin role" };

            var result = await _repository.AddAsync(role);

            var rolesInDb = await _context.Roles.ToListAsync();

            Assert.That(rolesInDb.Count, Is.EqualTo(1));
            Assert.That(rolesInDb[0].RoleName, Is.EqualTo(role.RoleName));
        }

        [Test]
        public async Task AddAsync_WhenRoleAlreadyExists_DoesNotCreateDuplicate()
        {
            var existingRole = new Role { RoleName = "Admin" };
            _context.Roles.Add(existingRole);
            await _context.SaveChangesAsync();

            var role = new Role { RoleName = "Admin" };

            var result = await _repository.AddAsync(role);

            var rolesInDb = await _context.Roles.ToListAsync();

            Assert.That(rolesInDb.Count, Is.EqualTo(1));
            Assert.That(result.RoleId, Is.EqualTo(existingRole.RoleId));
        }
    }
}
