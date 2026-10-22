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
            //services.AddScoped<ISearch, Search>();
            services.AddHttpClient<ISearch, Search>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Temporary local testing only. Trust Elasticsearch's CA certificate in production.
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           
            services.AddScoped<IAPI, API>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IUpload, Upload>();
            services.AddScoped<IDateTimeService, DateTimeService>();
            //services.AddSingleton<IElasticAuthApi, ElasticAuthApi>();
            services.AddHttpClient<IElasticAuthApi, ElasticAuthApi>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
            services.AddSingleton<LogBuffer>();

        }
    }
}