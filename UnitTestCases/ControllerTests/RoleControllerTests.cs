using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using Application.Controllers;
using Application.Database.Repositories.Interfaces;
using Application.Models;

namespace UnitTestCases.ControllerTests
{
    public class RoleControllerTests
    {
        private RoleController _controller = null!;
        private Mock<IRoleRepository> _roleRepoMock = null!;
        private Mock<IStringLocalizer<RoleController>> _localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            _roleRepoMock = new Mock<IRoleRepository>();
            _localizerMock = new Mock<IStringLocalizer<RoleController>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            _controller = new RoleController(
                _roleRepoMock.Object,
                _localizerMock.Object
            );
        }

        [Test]
        public async Task GetAllRoles_WhenCall_ReturnsOk()
        {
            List<Role> roles =
            [
                new() { RoleId = 1, RoleName = "Admin" },
                new() { RoleId = 2, RoleName = "User" }
            ];

            _roleRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(roles);

            var result = await _controller.GetAllRoles();

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(roles, Is.EqualTo(okResult!.Value));
        }

        [Test]
        public async Task GetRoleById_WhenPassId_ReturnsOk()
        {
            var role = new Role { RoleId = 1, RoleName = "Admin" };

            _roleRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(role);

            var result = await _controller.GetRoleById(1);

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(role, Is.EqualTo(okResult!.Value));
        }

        [Test]
        public async Task GetRoleById_WhenNotFound_ReturnsNotFound()
        {
            _roleRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync((Role?)null);

            var result = await _controller.GetRoleById(1);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateRole_WhenModelInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("RoleName", "Required");

            var role = new Role();

            var result = await _controller.CreateRole(role);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateRole_WhenValid_ReturnsOk()
        {
            var role = new Role { RoleName = "Admin", Description = "Admin role" };

            _roleRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Role>()))
                .ReturnsAsync(role);

            var result = await _controller.CreateRole(role);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateRole_WhenRoleNotFound_ReturnsNotFound()
        {
            _roleRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync((Role?)null);

            var role = new Role { RoleName = "Updated" };

            var result = await _controller.UpdateRole(1, role);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateRole_WhenRoleExists_ReturnsOk()
        {
            var existing = new Role { RoleId = 1, RoleName = "AdminUpdated", Description = "Old" };
            var updated = new Role { RoleName = "AdminUpdated", Description = "New" };

            _roleRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(existing);

            _roleRepoMock
                .Setup(r => r.AddAsync(existing))
                .ReturnsAsync(existing);

            var result = await _controller.UpdateRole(1, updated);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}