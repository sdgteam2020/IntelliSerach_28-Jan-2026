using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicsLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            return services;
        }
    }
}