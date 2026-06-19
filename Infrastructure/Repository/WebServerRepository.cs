using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repository
{
    public class WebServerRepository : GenericRepository<TrnWebServer>, IWebServer
    {
        protected new readonly ApplicationDbContext _context;

        public WebServerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> SoftDeleteWebServer(int Id)
        {
            var webServer = await _context.trnWebServer
                                .FirstOrDefaultAsync(x => x.Id == Id);

            if (webServer == null)
                return false;

            webServer.IsActive = false;
            webServer.IsDeleted = true;


            await _context.SaveChangesAsync();

            return true;

        }
    }
}
