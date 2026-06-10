using DataAccessLayer.ScraperSetting;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogicsLayer.ScraperSettings
{
    public class ScraperSetting : IWebScraperSetting
    {
        private readonly IWebScraperSettingDB _webScraperSettingDB;
        public ScraperSetting(IWebScraperSettingDB webScraperSettingDB)
        {
            _webScraperSettingDB= webScraperSettingDB;
        }

        public async Task<WebScraperSetting> GetById(int Id)
        {
            return await _webScraperSettingDB.GetById(Id);
        }

        public async Task<IEnumerable<WebScraperSetting>> GetWebScraperSetting()
        {
            
            return await _webScraperSettingDB.GetAll(); ;
        }

        public async Task<WebScraperSetting> UpdateWebScraperSetting(WebScraperSetting webScraperSetting)
        {
            if (webScraperSetting.Id == 0)
            {
                return await _webScraperSettingDB.AddWithReturn(webScraperSetting);
            }
            else if (webScraperSetting.Id > 0)
            {
                return await _webScraperSettingDB.UpdateWithReturn(webScraperSetting);
            }
            return webScraperSetting;
            
        }

        
    }
}
