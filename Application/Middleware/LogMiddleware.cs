using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Localization;

namespace Application.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogMiddleware> _logger;
        private readonly IStringLocalizer<LogMiddleware> _localizer;

        public LogMiddleware(
            RequestDelegate next,
            ILogger<LogMiddleware> logger,
            IStringLocalizer<LogMiddleware> localizer)
        {
            _next = next;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var requestBody = await ReadRequestBody(context);

                _logger.LogInformation(
                    "Request: {method} {url} \nBody: {body}",
                    context.Request.Method,
                    context.Request.Path,
                    requestBody);

                var originalBody = context.Response.Body;
                using var newBody = new MemoryStream();
                context.Response.Body = newBody;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Response: {statusCode} \nElapsedMs: {elapsed} \nBody: {body}",
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseBody);

                await newBody.CopyToAsync(originalBody);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    _localizer["UnhandledExceptionLog"],
                    ex.Message,
                    stopwatch.ElapsedMilliseconds);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    success = false,
                    message = _localizer["InternalServerError"]
                };

                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(response));
            }
        }

        private async Task<string> ReadRequestBody(HttpContext context)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
            context.Request.Body.Position = 0;
            return body;
        }
    }
}
