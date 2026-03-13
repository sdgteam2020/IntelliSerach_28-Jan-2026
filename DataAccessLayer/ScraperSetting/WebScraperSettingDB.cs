using DataAccessLayer.GenericRepository;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.ScraperSetting
{
    public class WebScraperSettingDB : GenericRepository<WebScraperSetting>, IWebScraperSettingDB
    {
        protected new readonly ApplicationDbContext _context;

        public WebScraperSettingDB(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
