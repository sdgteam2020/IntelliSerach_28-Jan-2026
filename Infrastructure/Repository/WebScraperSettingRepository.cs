using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Repository.GenericRepository;
using Application.Interfaces.Repository;

namespace Infrastructure.Repository
{
    public class WebScraperSettingRepository : GenericRepository<WebScraperSetting>, IWebScraperSetting
    {
        protected new readonly ApplicationDbContext _context;

        public WebScraperSettingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
