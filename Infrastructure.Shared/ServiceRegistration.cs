using Application.Interfaces;
using Infrastructure.Shared.Helpers;
using Infrastructure.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IAESEncrytDecry, AESEncrytDecry>();
            services.AddScoped<ISearch, Search>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
           
            services.AddScoped<IAPI, API>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IUpload, Upload>();
            services.AddScoped<IDateTimeService, DateTimeService>();
           


        }
    }
}