using Application.Interfaces.Repository;
using Domain.DTOs.Response;
using Domain.Entities;
using Domain.Enums;
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

        public async Task<TrnWebServer> AddWebServer(TrnWebServer Data)
        {
            if (Data.Id == 0)
            {
                return await AddWithReturn(Data);
            }
            else if (Data.Id > 0)
            {
                await UpdateWithReturn(Data);
                return Data;
            }
            return Data;
        }

        public async Task<List<DTOWebServerResponse>> GetAllActive(int UserId)
        {
            return await _context.trnWebServer
        .Where(i => i.IsActive && !i.IsDeleted && i.UpdatedBy == UserId)
        .Select(i => new DTOWebServerResponse
        {
            Id = i.Id,
            Url = i.Url,
            Includes = i.Includes,
            Index_Name = i.Index_Name
        })
        .ToListAsync();
        }

        public async Task<bool> SoftDeleteWebServer(int Id,int UserId)
        {
            var webServer = await _context.trnWebServer
                               .FirstOrDefaultAsync(x => x.Id == Id && x.UpdatedBy== UserId);

            if (webServer == null)
                return false;

            webServer.IsActive = false;
            webServer.IsDeleted = true;


            await _context.SaveChangesAsync();

            return true;

        }
    }
}
