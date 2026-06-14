using AIDocSearch.Interfaces;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIDocSearch.Services
{
    public static class ServiceRegistration
    {
        public static void AddSelfInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IEncryptionService, EncryptionService>();

            // Register new application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<ISessionService, SessionService>();
            services.TryAddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}
