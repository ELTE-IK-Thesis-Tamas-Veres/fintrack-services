using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
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

        public async Task Invoke(HttpContext context, IServiceScopeFactory serviceScopeFactory)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            string idToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            Console.WriteLine("ggggggggggggggg");

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(idToken))
            {
                var token = handler.ReadJwtToken(idToken);
                var sub = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                        ?? token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (!string.IsNullOrEmpty(sub))
                {
                    User? user = await userRepository.FindBySubAsync(sub, CancellationToken.None);

                    if (user == null)
                    {
                        user = new User { Sub = sub };

                        userRepository.Insert(user);
                        await userRepository.SaveAsync(CancellationToken.None);
                    }

                    context.Items["userId"] = user.Id;
                }
            }
            
            await _next(context);
        }
    }
}
