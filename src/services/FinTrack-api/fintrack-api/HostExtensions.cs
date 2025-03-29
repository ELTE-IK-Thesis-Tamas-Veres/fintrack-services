using Microsoft.EntityFrameworkCore;

namespace fintrack_api
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host) where TContext : DbContext
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<TContext>();
                context.Database.Migrate();
            }
            return host;
        }
    }

}
