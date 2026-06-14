using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Repository.GenericRepository;
using Application.Interfaces.Repository;

namespace Infrastructure.Repository
{
    public class WebServerRepository : GenericRepository<TrnWebServer>, IWebServer
    {
        protected new readonly ApplicationDbContext _context;

        public WebServerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
