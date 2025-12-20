using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Application.Database;
using Application.Database.Repositories;
using Application.Database.Repositories.Interfaces;
using Application.TokenGeneration;
using System.Globalization;

namespace UnitTestCases.ProgramTests
{
    public class ProgramConfigurationTests
    {
        private ServiceCollection _services = null!;

        [SetUp]
        public void SetUp()
        {
            _services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                   { "Jwt:Key", "unit-test-secret-key" },
                   { "Jwt:Issuer", "TestIssuer" },
                   { "Jwt:Audience", "TestAudience" },
                   { "Jwt:ExpiryMinutes", "20" }
                })
                .Build();

            _services.AddSingleton<IConfiguration>(configuration);

            _services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            _services.AddDbContext<DatabaseContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            _services.AddScoped<IUserRepository, UserRepository>();
            _services.AddScoped<IRoleRepository, RoleRepository>();
            _services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
            _services.AddSingleton<IUserSessionRepository, UserSessionRepository>();

            _services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = new[]
                {
            new CultureInfo("en"),
            new CultureInfo("hi-IN")
            };
                options.SupportedUICultures = options.SupportedCultures;
            });

            _services.AddDistributedMemoryCache();
            _services.AddSession();
            _services.AddAuthorization();
        }

        [Test]
        public void LocalizationOptions_ShouldHaveSupportedCultures()
        {
            var provider = _services.BuildServiceProvider();

            var options = provider
                .GetRequiredService<IOptions<RequestLocalizationOptions>>()
                .Value;

            Assert.Multiple(() =>
            {
                Assert.That(options.DefaultRequestCulture.Culture.Name, Is.EqualTo("en"));
                Assert.That(options.SupportedCultures?.Count, Is.EqualTo(2));
            });
            
        }

        [Test]
        public void DbContext_ShouldResolveSuccessfully()
        {
            var provider = _services.BuildServiceProvider();

            var dbContext = provider.GetService<DatabaseContext>();

            Assert.That(dbContext, Is.Not.Null);
        }
    }
}
