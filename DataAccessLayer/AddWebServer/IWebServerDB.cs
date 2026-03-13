using DataAccessLayer.GenericRepository;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.AddWebServer
{
    public interface IWebServerDB : IGenericRepository<TrnWebServer>
    {
    }
}
