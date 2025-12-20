using Application.Middleware;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using System.Text;

namespace UnitTestCases.MiddlewareTests
{
    public class LogMiddleWareTests
    {
        private Mock<ILogger<LogMiddleware>> _loggerMock = null!;
        private Mock<IStringLocalizer<LogMiddleware>> _localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<LogMiddleware>>();
            _localizerMock = new Mock<IStringLocalizer<LogMiddleware>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));
        }

        private static DefaultHttpContext CreateHttpContext(
            string requestBody = "{}")
        {
            var context = new DefaultHttpContext();

            context.Request.Body = new MemoryStream(
                Encoding.UTF8.GetBytes(requestBody));

            context.Response.Body = new MemoryStream();

            return context;
        }

        private static async Task<string> ReadResponse(
            HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(context.Response.Body)
                .ReadToEndAsync();
        }

        [Test]
        public async Task InvokeAsync_WhenRequestAndResponseAreSuccessful_LogsInformation()
        {
            var context = CreateHttpContext("{ \"name\": \"test\" }");

            var middleware = new LogMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    await ctx.Response.WriteAsync("Success");
                },
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(
                context.Response.StatusCode,
                Is.EqualTo(StatusCodes.Status200OK));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task InvokeAsync_WhenRequestBodyIsEmpty_DoesNotThrow()
        {
            var context = CreateHttpContext(string.Empty);

            var middleware = new LogMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                    await Task.CompletedTask;
                },
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(
                context.Response.StatusCode,
                Is.EqualTo(StatusCodes.Status204NoContent));
        }

        [Test]
        public async Task InvokeAsync_AlwaysRestoresOriginalResponseBody()
        {
            var context = CreateHttpContext();
            var originalBody = context.Response.Body;

            var middleware = new LogMiddleware(
                async ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    await ctx.Response.WriteAsync("OK");
                },
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(
                ReferenceEquals(context.Response.Body, originalBody),
                Is.False);
        }
    }
}
