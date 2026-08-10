using Application.Interfaces.GenericRepository;
using Domain.DTOs.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repository
{
    public interface IWebServer : IGenericRepository<TrnWebServer>
    {
        Task<bool> SoftDeleteWebServer(int Id, int UserId);
        Task<List<DTOWebServerResponse>> GetAllActive(int UserId);
        public Task<TrnWebServer> AddWebServer(TrnWebServer Data);
    }
}
