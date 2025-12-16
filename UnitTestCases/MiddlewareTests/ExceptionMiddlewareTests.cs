using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Net;
using UserRoleMangement.Middleware;

namespace UnitTestCases.MiddlewareTests
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;

        public ExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
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
        public async Task InvokeAsync_WhenCalled_ThorwsDbUpdateException()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new DbUpdateException("DB error"),
                _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That((int)HttpStatusCode.BadRequest, Is.EqualTo(context.Response.StatusCode));

            var response = await ReadResponse(context);
            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public async Task InvokeAsync_WhenCalled_ReturnArgumentNullException()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new ArgumentNullException("userId"),
                _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That((int)HttpStatusCode.BadRequest, Is.EqualTo(context.Response.StatusCode));

            var response = await ReadResponse(context);
            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public async Task InvokeAsync_WhenCalled_KeyNotFoundException()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new KeyNotFoundException("User not found"),
                _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That((int)HttpStatusCode.NotFound, Is.EqualTo(context.Response.StatusCode));
        }

        [Test]
        public async Task InvokeAsync_UnauthorizedAccessException_ReturnsUnauthorized()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new UnauthorizedAccessException(),
                _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That((int)HttpStatusCode.Unauthorized, Is.EqualTo(context.Response.StatusCode));
        }

      
        [Test]
        public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
        {
            var context = CreateHttpContext();

            var middleware = new ExceptionMiddleware(
                _ => throw new Exception("Something went wrong"),
                _loggerMock.Object);

            await middleware.InvokeAsync(context);

            Assert.That((int)HttpStatusCode.InternalServerError, Is.EqualTo(context.Response.StatusCode));

            var response = await ReadResponse(context);
            Assert.That(response, Is.Not.Null);
        }
    }
}
