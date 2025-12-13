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
            var path = context.Request.Path.Value?.ToLower();

            if (context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Delete)
            {
             
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    string toeknerror = "Authorization token missing";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($"{{\"message\":\"{toeknerror}\"}}");
                    return;
                }

                var requestToken = authHeader.Replace("Bearer ", "").Trim();

                var sessionToken = context.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(sessionToken))
                {
                    string sessionExpired = "Session expired. Please login again.";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($"{{\"message\":\"{sessionExpired}\"}}");
                    return;
                }

                if (requestToken != sessionToken)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($"{{\"message\":\"Token mismatch. Invalid session.\"}}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
