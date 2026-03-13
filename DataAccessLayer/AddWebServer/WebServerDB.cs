using DataAccessLayer.GenericRepository;
using DataAccessLayer.UploadFiles;
using DataTransferObject.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.AddWebServer
{
    public class WebServerDB : GenericRepository<TrnWebServer>, IWebServerDB
    {
        protected new readonly ApplicationDbContext _context;

        public WebServerDB(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
