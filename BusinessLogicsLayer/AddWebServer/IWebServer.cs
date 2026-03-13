using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicsLayer.AddWebServer
{
    public interface IWebServer
    {
        public Task<TrnWebServer> AddWebServer(TrnWebServer Data);
        public Task<TrnWebServer> GetById(int Id);
        public Task<IEnumerable<TrnWebServer>> GetAll();
    }
}
