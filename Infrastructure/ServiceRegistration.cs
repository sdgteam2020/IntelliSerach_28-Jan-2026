using Application.Interfaces.Repository;
using Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
         
            services.AddScoped<IUserAccount, AccountRepository>();
            services.AddScoped<IRank, RankRepository>();
            services.AddScoped<IUploadFiles, UploadFilesRepository>();
            services.AddScoped<IWebServer, WebServerRepository>();
            services.AddTransient<ILoger, LogerRepository>();
            services.AddScoped<IWebScraperSetting, WebScraperSettingRepository>();

        }
    }
}
