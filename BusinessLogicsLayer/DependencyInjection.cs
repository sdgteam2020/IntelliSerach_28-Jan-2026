using BusinessLogicsLayer.Accounts;
using BusinessLogicsLayer.AddWebServer;
using BusinessLogicsLayer.Logers;
using BusinessLogicsLayer.Ranks;
using BusinessLogicsLayer.ScraperAPI;
using BusinessLogicsLayer.ScraperSettings;
using BusinessLogicsLayer.SearchContent;
using BusinessLogicsLayer.Service;
using BusinessLogicsLayer.UnitOfWorks;
using BusinessLogicsLayer.UploadPdf;
using DataAccessLayer.Account;
using DataAccessLayer.AddWebServer;
using DataAccessLayer.Logers;
using DataAccessLayer.ScraperSetting;
using DataAccessLayer.UploadFiles;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicsLayer
{
    public static class DependencyInjection
    {
        public static void AddRepository(this IServiceCollection services)
        {
            services.AddTransient<ISearch, Search>();

            services.AddTransient<IAccount, Account>();
            services.AddTransient<IAccountDL, AccountDL>();

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IRank, Rank>();
            services.AddTransient<IAPI, API>();
            services.AddTransient<IService, ServiceRepository>();
            services.AddTransient<IUploadFiles, UploadFiles>();
            services.AddTransient<IUploadFilesDB, UploadFilesDB>();

            services.AddTransient<IWebServer, WebServer>();
            services.AddTransient<IWebServerDB, WebServerDB>();

            services.AddTransient<ILoger, Loger>();
            services.AddTransient<ILogerDB, LogerDB>();
            services.AddTransient<IWebScraperSetting, ScraperSetting>();
            services.AddTransient<IWebScraperSettingDB, WebScraperSettingDB>();

        }
    }
}