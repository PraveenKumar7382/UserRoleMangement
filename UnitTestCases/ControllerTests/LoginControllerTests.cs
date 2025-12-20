using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using Application.Controllers;
using Application.Database.Repositories.Interfaces;
using Application.Models;
using Application.TokenGeneration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace UnitTestCases.ControllerTests
{
    public class LoginControllerTests
    {
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private Mock<IUserSessionRepository> _sessionRepoMock = null!;
        private Mock<IJwtTokenHelper> _jwtTokenHelperMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<IStringLocalizer<LoginController>> _localizerMock = null!;
        private LoginController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _sessionRepoMock = new Mock<IUserSessionRepository>();
            _jwtTokenHelperMock = new Mock<IJwtTokenHelper>();
            _configurationMock = new Mock<IConfiguration>();
            _localizerMock = new Mock<IStringLocalizer<LoginController>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            _configurationMock
                .Setup(x => x["Jwt:ExpiryMinutes"])
                .Returns("20");

            _controller = new LoginController(
                _userRepositoryMock.Object,
                _sessionRepoMock.Object,
                _jwtTokenHelperMock.Object,
                _configurationMock.Object,
                _localizerMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var login = new Login { UserName = "test", PasswordHash = "password" };
            var user = new User { UserId = 1, UserName = "test" };

            _userRepositoryMock
                .Setup(x => x.LoginAsync(login))
                .ReturnsAsync((true, "Success", user));

            _sessionRepoMock
                .Setup(x => x.GetActiveSessionForUserAsync(user.UserId))
                .ReturnsAsync((UserSession?)null);

            _jwtTokenHelperMock
                .Setup(x => x.GenerateToken(user))
                .Returns("access_token");

            var result = await _controller.Login(login);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var login = new Login { UserName = "test", PasswordHash = "wrong" };

            _userRepositoryMock
                .Setup(x => x.LoginAsync(login))
                .ReturnsAsync((false, "Invalid", null));

            var result = await _controller.Login(login);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Login_WhenUserAlreadyLoggedIn_ReturnsUnauthorized()
        {
            var login = new Login { UserName = "test", PasswordHash = "password" };
            var user = new User { UserId = 1 };

            _userRepositoryMock
                .Setup(x => x.LoginAsync(login))
                .ReturnsAsync((true, "Success", user));

            _sessionRepoMock
                .Setup(x => x.GetActiveSessionForUserAsync(user.UserId))
                .ReturnsAsync(new UserSession());

            var result = await _controller.Login(login);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        
        [Test]
        public async Task Logout_WhenSessionNotFound_ReturnsUnauthorized()
        {
            var guid = Guid.NewGuid();

            var result = await _controller.Logout(guid);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}