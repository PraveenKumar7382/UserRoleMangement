using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using UserRoleMangement.Database.Repositories.Interfaces;
using UserRoleMangement.TokenGeneration;

namespace UserRoleMangement.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Delete)
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await Unauthorized(context, "Authorization token missing");
                    return;
                }

                var token = authHeader.Replace("Bearer ", "").Trim();

                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken;
                try
                {
                    jwtToken = handler.ReadJwtToken(token);
                }
                catch
                {
                    await Unauthorized(context, "Invalid token format");
                    return;
                }

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    await Unauthorized(context, "Token does not contain valid user information");
                    return;
                }

                var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
                var user = await userRepo.GetById(userId);
                if (user == null)
                {
                    await Unauthorized(context, "User does not exist. Invalid token.");
                    return;
                }

                var tokenStore = context.RequestServices.GetRequiredService<IUserTokenStore>();
                if (!tokenStore.IsValidToken(userId, token))
                {
                    await Unauthorized(context, "Token is not valid for this user.");
                    return;
                }
            }

            await _next(context);
        }

        private static async Task Unauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { success = false, message }));
        }
    }
}
