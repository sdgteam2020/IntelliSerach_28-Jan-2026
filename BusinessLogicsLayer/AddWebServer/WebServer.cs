using BusinessLogicsLayer.Service;
using DataAccessLayer.AddWebServer;
using DataAccessLayer.UploadFiles;
using DataTransferObject.DTO.Requests;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicsLayer.AddWebServer
{
    public class WebServer : IWebServer
    {

        private readonly IWebServerDB _webServerDB;

        public WebServer(IService service, IWebServerDB webServerDB)
        {
          
            _webServerDB = webServerDB;
        }

        public async Task<TrnWebServer> AddWebServer(TrnWebServer Data)
        {
           
            if (Data.Id==0)
            {
                return await _webServerDB.AddWithReturn(Data);
            }
            else if (Data.Id > 0)
            {
                await _webServerDB.UpdateWithReturn(Data);
                return Data;
            }
            return Data;
        }

        public async Task<IEnumerable<TrnWebServer>> GetAll()
        {
            return await _webServerDB.GetAll();
        }

        public Task<TrnWebServer> GetById(int Id)
        {
            return _webServerDB.GetById(Id);
        }
    }
}
