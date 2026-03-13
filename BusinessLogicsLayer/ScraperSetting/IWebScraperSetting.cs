using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicsLayer.ScraperSetting
{
    public interface IWebScraperSetting
    {
        Task<IEnumerable<WebScraperSetting>> GetWebScraperSetting();
        Task<WebScraperSetting> UpdateWebScraperSetting(WebScraperSetting webScraperSetting);
        Task<WebScraperSetting> GetById(int Id);
    }
}
