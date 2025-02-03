using System.Security.Claims;

namespace fintrack_api.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["UserId"] = userId;
            }

            await _next(context);
        }
    }
}
