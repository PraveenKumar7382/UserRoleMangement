using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace UserRoleMangement.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
            int statusCode = (int)HttpStatusCode.InternalServerError; 
            string message = "An unexpected error occurred. Please try again later.";

            switch (exception)
            {
                case DbUpdateException dbEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = "Database operation failed. Please check the input or contact support.";
                    break;

                case ArgumentNullException argNullEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = $"Missing argument: {argNullEx.ParamName}";
                    break;

                case ArgumentException argEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = invOpEx.Message;
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = keyNotFoundEx.Message;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = "You are not authorized to perform this action.";
                    break;

                case ValidationException validationEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = validationEx.Message;
                    break;

                case NotImplementedException notImplEx:
                    statusCode = (int)HttpStatusCode.NotImplemented;
                    message = "This feature is not implemented.";
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            _logger.LogError(exception, $"Exception caught by middleware: {exception.Message}");
            var response = new
            {
                success = false,
                statusCode,
                message
            };

            context.Response.StatusCode = statusCode;
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
