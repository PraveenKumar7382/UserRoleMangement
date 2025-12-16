using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using UserRoleMangement.Models;
using UserRoleMangement.TokenGeneration;

namespace UnitTestCases.Tokengeneration
{
    public class JwtToeknHelperTests
    {
        private JwtTokenHelper CreateHelper()
        {
            var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Key", "this_is_a_very_secure_secret_key_123456" },
            { "Jwt:ExpiryMinutes", "5" }
        };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return new JwtTokenHelper(configuration);
        }

        [Test]
        public void GenerateToken_WithValidUser_ReturnsJwtToken()
        {
            var helper = CreateHelper();
            var user = new User
            {
                UserId = 10,
                UserName = "Admin"
            };

            var token = helper.GenerateToken(user);

            Assert.That(token, Is.Not.Null);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.That(jwt.Claims.First(c => c.Type == "userId").Value, Is.EqualTo("10"));
        }

        [Test]
        public void GenerateToken_ContainExpiry()
        {
            var helper = CreateHelper();
            var user = new User { UserId = 1, UserName = "Test" };
            
            var token = helper.GenerateToken(user);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.That(jwt.ValidTo, Is.Not.Null);
            Assert.That(jwt.ValidTo > DateTime.UtcNow, Is.True);
        }

        [Test]
        public void GenerateToken_WhenKeyIsShort_GenerateToken()
        {
            var settings = new Dictionary<string, string>
        {
            { "Jwt:Key", "shortkey" }, 
            { "Jwt:ExpiryMinutes", "2" }
        };

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            var helper = new JwtTokenHelper(config);

            var user = new User { UserId = 5, UserName = "User" };

            var token = helper.GenerateToken(user);

            Assert.That(token, Is.Not.Null);
        }

        [Test]
        public void GenerateToken_WhenConfigurationMissing_ThrowsException()
        {
            IConfiguration config = new ConfigurationBuilder().Build();
            var helper = new JwtTokenHelper(config);

            var user = new User { UserId = 1, UserName = "Test" };

            var ex = Assert.Throws<InvalidOperationException>(() =>
                helper.GenerateToken(user)
            );

            Assert.That(ex.Message, Is.Not.Null);
        }
    }
}
