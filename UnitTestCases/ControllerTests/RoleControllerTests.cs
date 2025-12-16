//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using NUnit.Framework;
//using UserRoleMangement.Controllers;
//using UserRoleMangement.Database.Repositories.Interfaces;
//using UserRoleMangement.Models;

//namespace UnitTestCases.ControllerTests
//{
//    public class RoleControllerTests
//    {
//        private RoleController _controller = null!;
//        private Mock<IRoleRepository> _roleRepoMock = null!;

//        [SetUp]
//        public void Setup()
//        {
//            _roleRepoMock = new Mock<IRoleRepository>();
//            _controller = new RoleController(_roleRepoMock.Object);
//        }

//        [Test]
//        public async Task GetAllRoles_WhenCall_ReturnsOk()
//        {
//            List<Role> roles =
//            [
//            new() { RoleId = 1, RoleName = "Admin" },
//            new() { RoleId = 2, RoleName = "User" }
//            ];

//            _roleRepoMock
//                .Setup(r => r.GetAllAsync())
//                .ReturnsAsync(roles);

//            var result = await _controller.GetAllRoles();

//            var okResult = result as OkObjectResult;
//            Assert.NotNull(okResult);
//            Assert.AreEqual(roles, okResult!.Value);
//        }

//        [Test]
//        public async Task GetRoleById_WhenPassId_ReturnsOk()
//        {
//            var role = new Role { RoleId = 1, RoleName = "Admin" };

//            _roleRepoMock
//                .Setup(r => r.GetById(1))
//                .ReturnsAsync(role);

//            var result = await _controller.GetRoleById(1);

//            var okResult = result as OkObjectResult;
//            Assert.NotNull(okResult);
//            Assert.AreEqual(role, okResult!.Value);
//        }

//        [Test]
//        public async Task GetRoleById_WhenPassId_RetursExpectedResponse()
//        {
//            _roleRepoMock
//                .Setup(r => r.GetById(1))!
//                .ReturnsAsync((Role?)null);

//            var result = await _controller.GetRoleById(1);

//            var notFound = result as NotFoundObjectResult;
//            Assert.NotNull(notFound);
//            Assert.AreEqual("Role not found", notFound!.Value);
//        }

//        [Test]
//        public async Task CreateRole_WhenRoleIsInValid_ReturnModelIsInaValidState()
//        {
//            _controller.ModelState.AddModelError("RoleName", "Required");

//            var role = new Role();

//            var result = await _controller.CreateRole(role);

//            Assert.IsInstanceOf<BadRequestObjectResult>(result);
//        }

//        [Test]
//        public async Task CreateRole_WhenRoleIsValid_ReturnsOk()
//        {
//            var role = new Role { RoleName = "Admin", Description = "Admin role" };

//            _roleRepoMock
//                .Setup(r => r.AddAsync(role))
//                .ReturnsAsync(role);

//            var result = await _controller.CreateRole(role);

//            var okResult = result as OkObjectResult;
//            Assert.NotNull(okResult);
//            Assert.AreEqual(role, okResult!.Value);
//        }

//        [Test]
//        public async Task UpdateRole_WhenPassIdWithRole_ReturnsNotFound()
//        {
//            _roleRepoMock
//                .Setup(r => r.GetById(1))!
//                .ReturnsAsync((Role?)null);

//            var role = new Role { RoleName = "Updated" };

//            var result = await _controller.UpdateRole(1, role);

//            var notFound = result as NotFoundObjectResult;
//            Assert.That(notFound, Is.Not.Null);
//        }

//        [Test]
//        public async Task UpdateRole_WhenPassIdWithRoleWithExist_ReturnsOk()
//        {
//            var existing = new Role { RoleId = 1, RoleName = "Admin", Description = "Old" };
//            var updated = new Role { RoleName = "AdminUpdated", Description = "New" };

//            _roleRepoMock
//                .Setup(r => r.GetById(1))
//                .ReturnsAsync(existing);

//            _roleRepoMock
//                .Setup(r => r.AddAsync(existing))
//                .ReturnsAsync(existing);

//            var result = await _controller.UpdateRole(1, updated);

//            var okResult = result as OkObjectResult;
//            Assert.That(okResult, Is.Not.Null);
//            Assert.That(existing, Is.EqualTo(okResult!.Value));
//            Assert.That(updated.RoleName, Is.EqualTo(existing.RoleName));
//            Assert.That(updated.Description, Is.EqualTo(existing.Description));
//        }
//    }
//}
