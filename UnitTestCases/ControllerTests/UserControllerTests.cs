using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using UserRoleMangement.Controllers;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.Models;

namespace UnitTestCases.ControllerTests
{
    public class UserControllerTests
    {
        private UserController _controller = null!;
        private Mock<IUserRepository> _userRepoMock = null!;

        [SetUp]
        public void Setup()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _controller = new UserController(_userRepoMock.Object);
        }

        [Test]
        public async Task GetAll_WhenCalled_ReturnsOk()
        {
            List<User> users =
            [
            new User { UserId = 1, UserName = "Admin" },
            new User { UserId = 2, UserName = "User" }
            ];

            _userRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            var result = await _controller.Get();

            var okResult = result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(users, Is.EqualTo(okResult!.Value));
            });
        }

        [Test]
        public async Task GetById_WhenUserExists_ReturnsOk()
        {
            var user = new User { UserId = 1, UserName = "Admin" };

            _userRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(user);

            var result = await _controller.Get(1);

            var okResult = result as OkObjectResult;

            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null);
                Assert.That(user, Is.EqualTo(okResult!.Value));
            });
        }

        [Test]
        public async Task GetById_WhenUserDoesNotExist_ReturnsNotFound()
        {
            _userRepoMock
                .Setup(r => r.GetById(1))!
                .ReturnsAsync((User?)null);

            var result = await _controller.Get(1);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Create_WhenUserCreationFails_ReturnsBadRequest()
        {
            var user = new User { UserName = "Test" };

            _userRepoMock
                .Setup(r => r.AddAsync(user))
                .ReturnsAsync((false, "User exists", null));

            var result = await _controller.Create(user);

            var badRequest = result as BadRequestObjectResult;

            Assert.NotNull(badRequest);
        }

        [Test]
        public async Task Create_WhenUserCreatedSuccessfully_ReturnsOk()
        {
            var user = new User
            {
                UserId = 1,
                UserName = "Admin",
                Email = "admin@test.com",
                Role = new Role { RoleName = "Admin", Description = "Admin role" }
            };

            _userRepoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((true, "User added successfully", user));

            var result = await _controller.Create(user);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
        }

        [Test]
        public async Task ForgotPassword_WhenUserNotFound_ReturnsBadRequest()
        {
            var request = new ForgotPassword { UserName = "test" };

            _userRepoMock
                .Setup(r => r.ForgotPasswordAsync(request))
                .ReturnsAsync((false, "User Name does not exist"));

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WhenSuccess_ReturnsOk()
        {
            var request = new ForgotPassword { UserName = "test" };

            _userRepoMock
                .Setup(r => r.ForgotPasswordAsync(request))
                .ReturnsAsync((true, "Password updated"));

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_WhenUserDoesNotExist_ReturnsNotFound()
        {
            var user = new User { UserName = "Test" };

            _userRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>()))!
                .ReturnsAsync((User?)null);

            var result = await _controller.Update(1, user);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Update_WhenUserExists_ReturnsOk()
        {
            var user = new User
            {
                UserId = 1,
                UserName = "Updated",
                Email = "updated@test.com"
            };

            _userRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(user);

            var result = await _controller.Update(1, user);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Delete_WhenUserDoesNotExist_ReturnsNotFound()
        {
            _userRepoMock
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(false);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task Delete_WhenUserExists_ReturnsOk()
        {
            _userRepoMock
                .Setup(r => r.DeleteAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
