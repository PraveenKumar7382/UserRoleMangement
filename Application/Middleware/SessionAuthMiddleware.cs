using Application.Database.Repositories.Interfaces;
using Application.Models;
using Microsoft.Extensions.Localization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;

public class SessionAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<SessionAuthMiddleware> _localizer;

    public SessionAuthMiddleware(
        RequestDelegate next,
        IStringLocalizer<SessionAuthMiddleware> localizer)
    {
        _next = next;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context, IUserSessionRepository userSessionRepository)
    {
        try
        {
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon") ||
                context.Request.Path.StartsWithSegments("/openapi") || 
                context.Request.Path.StartsWithSegments("/api/login") ||
                context.Request.Path.StartsWithSegments("/api/user/create") ||
                context.Request.Path.StartsWithSegments("/api/user/forgotpassword") ||
                context.Request.Path.StartsWithSegments("/api/role"))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                await Unauthorized(context, _localizer["AuthTokenMissing"]);
                return;
            }

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(authHeader["Bearer ".Length..]);
            }
            catch
            {
                await Unauthorized(context, _localizer["InvalidToken"]);
                return;
            }

            int userId = int.Parse(jwt.Claims.First(c => c.Type == "userId").Value);
            string roleName = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;

            var lastSession = await userSessionRepository.GetActiveSessionForUserAsync(userId);

            if (lastSession == null)
            {
                await Unauthorized(context, _localizer["SessionExpiredOrLoggedOut"]);
                return;
            }

            if (lastSession.ExpiresAt < DateTime.UtcNow)
            {
                await userSessionRepository.DeleteSessionAsync(lastSession.UserId);
                await Unauthorized(context, _localizer["SessionExpired"]);
                return;
            }

            if (lastSession.Revoked)
            {
                await Unauthorized(context, _localizer["AlreadyLoggedInElsewhere"]);
                return;
            }

            context.Items["UserId"] = userId;
            context.Items["Role"] = roleName;
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = _localizer["UnexpectedError"] });
        }
    }

    private static async Task Unauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
