using Application.Models;
using Application.TokenGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace UnitTestCases.TokenTests
{
    public class JwtTokenHelperTests
    {
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<IStringLocalizer<JwtTokenHelper>> _localizerMock = null!;
        private JwtTokenHelper _jwtTokenHelper = null!;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _localizerMock = new Mock<IStringLocalizer<JwtTokenHelper>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("MySuperSecretKeyForTesting123!");
            _configurationMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("5");

            _jwtTokenHelper = new JwtTokenHelper(_configurationMock.Object, _localizerMock.Object);
        }

        [Test]
        public void GenerateToken_ReturnsValidJwtToken()
        {
            var user = new User
            {
                UserId = 1,
                UserName = "testuser",
                Role = new Role { RoleName = "Admin" }
            };

            Guid guid = Guid.NewGuid();

            var tokenString = _jwtTokenHelper.GenerateToken(user, guid);

            Assert.That(tokenString, Is.Not.Null);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "userId");
            var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            Assert.That(user.UserId.ToString(), Is.EqualTo(userIdClaim?.Value));
            Assert.That(user.Role.RoleName, Is.EqualTo(roleClaim?.Value));
        }

        [Test]
        public void GenerateToken_KeyTooShort_HashesKeyAndGeneratesToken()
        {
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("shortkey");
            var user = new User
            {
                UserId = 2,
                UserName = "shortkeyuser",
                Role = new Role { RoleName = "User" }
            };
            Guid guid = Guid.NewGuid();
            var tokenString = _jwtTokenHelper.GenerateToken(user, guid);

            Assert.That(tokenString, Is.Not.Null);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            Assert.That(user.UserId.ToString(), Is.EqualTo(token.Claims.First(c => c.Type == "userId").Value));
        }

        [Test]
        public void GenerateToken_NullRole_ThrowsException()
        {
            var user = new User
            {
                UserId = 3,
                UserName = "noroleuser",
                Role = null
            };
            Guid guid = Guid.NewGuid();

            var ex = Assert.Throws<InvalidOperationException>(() => _jwtTokenHelper.GenerateToken(user, guid));
            Assert.That(ex.Message, Is.EqualTo("JwtGenerationFailed"));
        }
    }
}