using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using Application.Controllers;
using Application.Database.Repositories.Interfaces;
using Application.Models;

namespace UnitTestCases.ControllerTests
{
    public class UserControllerTests
    {
        private UserController _controller = null!;
        private Mock<IUserRepository> _userRepoMock = null!;
        private Mock<IStringLocalizer<UserController>> _localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _localizerMock = new Mock<IStringLocalizer<UserController>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            _controller = new UserController(
                _userRepoMock.Object,
                _localizerMock.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Items["Role"] = "admin";
            httpContext.Items["UserId"] = 1;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
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

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Me_WhenCalled_ReturnsOk()
        {
            var user = new User { UserId = 1, UserName = "Admin" };

            _userRepoMock
                .Setup(r => r.GetById(1))
                .ReturnsAsync(user);

            var result = await _controller.Me();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Create_WhenFails_ReturnsBadRequest()
        {
            var user = new User { UserName = "Test" };

            _userRepoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((false, "UserExists", null));

            var result = await _controller.Create(user);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Create_WhenSuccess_ReturnsOk()
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
                .ReturnsAsync((true, "UserCreated", user));

            var result = await _controller.Create(user);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WhenFails_ReturnsBadRequest()
        {
            var request = new ForgotPassword { UserName = "test" };

            _userRepoMock
                .Setup(r => r.ForgotPasswordAsync(request))
                .ReturnsAsync((false, "UserNotFound"));

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ForgotPassword_WhenSuccess_ReturnsOk()
        {
            var request = new ForgotPassword { UserName = "test" };

            _userRepoMock
                .Setup(r => r.ForgotPasswordAsync(request))
                .ReturnsAsync((true, "PasswordResetSuccess"));

            var result = await _controller.ForgotPassword(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_Admin_WhenNoChanges_ReturnsOk()
        {
            var user = new User { UserName = "Test" };

            _userRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User?)null);

            var result = await _controller.Update(1, user);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_Admin_WhenUpdated_ReturnsOk()
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
    }
}