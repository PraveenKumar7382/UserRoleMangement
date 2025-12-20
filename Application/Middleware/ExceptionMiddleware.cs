using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace Application.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IStringLocalizer<ExceptionMiddleware> _localizer;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IStringLocalizer<ExceptionMiddleware> localizer)
        {
            _next = next;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            int statusCode;
            string message;

            switch (exception)
            {
                case DbUpdateException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = _localizer["DatabaseUpdateFailed"];
                    break;

                case SqlException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = _localizer["SqlError"];
                    break;

                case ArgumentNullException argNullEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = string.Format(
                        _localizer["MissingArgument"],
                        argNullEx.ParamName);
                    break;

                case ArgumentException argEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = invOpEx.Message;
                    break;

                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = _localizer["ResourceNotFound"];
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = _localizer["Unauthorized"];
                    break;

                case ValidationException validationEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = validationEx.Message;
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = _localizer["UnexpectedError"];
                    break;
            }

            _logger.LogError(exception, "Exception caught by middleware");

            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                statusCode,
                message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
