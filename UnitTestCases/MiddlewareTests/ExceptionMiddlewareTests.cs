using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Net;
using Application.Middleware;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace UnitTestCases.MiddlewareTests
{
    public class ExceptionMiddlewareTests
    {
        private Mock<ILogger<ExceptionMiddleware>> _loggerMock = null!;
        private Mock<IStringLocalizer<ExceptionMiddleware>> _localizerMock = null!;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            _localizerMock = new Mock<IStringLocalizer<ExceptionMiddleware>>();

            _localizerMock
                .Setup(x => x[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));
        }

        private static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<string> ReadResponse(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return await new StreamReader(context.Response.Body).ReadToEndAsync();
        }

        [Test]
        public async Task InvokeAsync_DbUpdateException_ReturnsBadRequest()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new DbUpdateException("DB error"),
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("DatabaseUpdateFailed"));
        }

        [Test]
        public async Task InvokeAsync_ArgumentNullException_ReturnsBadRequest()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new ArgumentNullException("userId"),
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task InvokeAsync_KeyNotFoundException_ReturnsNotFound()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new KeyNotFoundException(),
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task InvokeAsync_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new UnauthorizedAccessException(),
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new Exception("Error"),
                _loggerMock.Object,
                _localizerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));

            var response = await ReadResponse(context);
            Assert.That(response, Does.Contain("UnexpectedError"));
        }
    }
}